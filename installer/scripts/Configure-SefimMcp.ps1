<#
.SYNOPSIS
    Configuration helper invoked by the Sefim MCP NSIS installer.

.DESCRIPTION
    NSIS cannot safely rewrite an arbitrary JSON document, so every step that has to
    keep foreign data intact is done here instead:

      Detect     report which MCP host applications and Sefim installations exist
      Parse      read Data Source / Initial Catalog / User ID / Password from
                 the Sefim connectionstring.txt
      Test       open a real SQL Server connection with the collected credentials
      Configure  write appsettings.json and merge the MCP server entry into the
                 host configuration file, leaving all other keys untouched
      Remove     delete the MCP server entry again (used by the uninstaller)

    Results are written as "key=value" lines, both to stdout and to the UTF-16LE
    file given by -ResultFile, which is what the installer reads back.

.NOTES
    Windows PowerShell 5.1 (shipped with Windows 10 and later) is the only requirement.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Detect', 'Parse', 'Test', 'Configure', 'Remove')]
    [string]$Mode,

    [string]$InstallDir,
    [ValidateSet('claude', 'chatgpt', '')]
    [string]$HostApp = '',
    [string]$SefimDir,
    [string]$Server,
    [string]$Database,
    [string]$UserId,
    [string]$Password,
    [string]$KnowledgeKey,
    [string]$ToolProfile = 'full',
    [string]$ServerKey = 'sefim',
    [string]$ExeName = 'sefim-ai-mcp.exe',

    # Values that may contain quotes, spaces or Unicode (passwords, paths) are handed
    # over in a UTF-16 key=value file instead of on the command line.
    [string]$InputFile,

    # The installer reads the result from this file; UTF-16 keeps non-ASCII paths intact.
    [string]$ResultFile
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Output helpers. The installer reads stdout line by line; stderr is ignored.
# ---------------------------------------------------------------------------

$script:ResultLines = New-Object System.Collections.Generic.List[string]

function Write-Pair {
    param([string]$Key, $Value)
    if ($null -eq $Value) { $Value = '' }
    $line = "{0}={1}" -f $Key, ([string]$Value -replace '[\r\n]+', ' ')
    $script:ResultLines.Add($line)
    Write-Output $line
}

function Save-Result {
    if (-not $ResultFile) { return }
    # UTF-16LE without a BOM: NSIS FileReadUTF16LE would otherwise return the BOM
    # as part of the first key.
    $encoding = New-Object System.Text.UnicodeEncoding($false, $false)
    [System.IO.File]::WriteAllLines($ResultFile, $script:ResultLines, $encoding)
}

function Fail {
    param([string]$Message)
    Write-Pair 'status' 'error'
    Write-Pair 'message' $Message
    Save-Result
    exit 1
}

function Import-InputFile {
    param([string]$Path)

    if (-not $Path -or -not (Test-Path -LiteralPath $Path)) { return }

    foreach ($line in (Get-Content -LiteralPath $Path -Encoding Unicode)) {
        if (-not $line -or $line.TrimStart().StartsWith('#')) { continue }
        $separator = $line.IndexOf('=')
        if ($separator -lt 1) { continue }

        $key = $line.Substring(0, $separator).Trim().ToLowerInvariant()
        $value = $line.Substring($separator + 1)

        switch ($key) {
            'installdir'   { $script:InstallDir = $value }
            'hostapp'      { $script:HostApp = $value }
            'sefimdir'     { $script:SefimDir = $value }
            'server'       { $script:Server = $value }
            'database'     { $script:Database = $value }
            'userid'       { $script:UserId = $value }
            'password'     { $script:Password = $value }
            'knowledgekey' { $script:KnowledgeKey = $value }
            'toolprofile'  { if ($value) { $script:ToolProfile = $value } }
            'serverkey'    { if ($value) { $script:ServerKey = $value } }
        }
    }
}

# ---------------------------------------------------------------------------
# User profile resolution.
#
# The installer runs elevated. With a same-user UAC consent the profile folders
# still belong to the interactive user, but when an administrator of a different
# account approves the prompt, $env:APPDATA points at that administrator. Both
# cases are handled: the current profile wins, otherwise the profile that
# actually owns a host configuration is used.
# ---------------------------------------------------------------------------

function Get-CandidateProfiles {
    $profiles = New-Object System.Collections.Generic.List[string]
    if ($env:USERPROFILE -and (Test-Path -LiteralPath $env:USERPROFILE)) {
        $profiles.Add($env:USERPROFILE)
    }

    $root = Split-Path -Parent ([Environment]::GetFolderPath('UserProfile'))
    if ($root -and (Test-Path -LiteralPath $root)) {
        Get-ChildItem -LiteralPath $root -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -notin @('Default', 'Default User', 'Public', 'All Users') } |
            Sort-Object LastWriteTime -Descending |
            ForEach-Object {
                if (-not $profiles.Contains($_.FullName)) { $profiles.Add($_.FullName) }
            }
    }

    return $profiles
}

function Get-HostConfigCandidates {
    param([string]$App, [string]$ProfilePath)

    $roaming = Join-Path $ProfilePath 'AppData\Roaming'
    switch ($App) {
        'claude' {
            return @(Join-Path $roaming 'Claude\claude_desktop_config.json')
        }
        'chatgpt' {
            # ChatGPT Desktop has no single documented stdio-MCP config file yet, so
            # every known location is probed and the first existing one is reused.
            return @(
                (Join-Path $roaming 'ChatGPT\mcp_config.json'),
                (Join-Path $roaming 'ChatGPT\config.json'),
                (Join-Path $roaming 'OpenAI\ChatGPT\mcp_config.json'),
                (Join-Path $roaming 'OpenAI\ChatGPT\config.json')
            )
        }
    }
    return @()
}

function Resolve-HostConfigPath {
    param([string]$App, [switch]$MustExist)

    foreach ($profilePath in Get-CandidateProfiles) {
        foreach ($candidate in Get-HostConfigCandidates -App $App -ProfilePath $profilePath) {
            if (Test-Path -LiteralPath $candidate) { return $candidate }
        }
    }

    if ($MustExist) { return '' }

    # Nothing on disk yet: create the canonical file inside the current profile.
    # @() is required: a single returned path would otherwise be indexed as a string.
    $profiles = @(Get-CandidateProfiles)
    $candidates = @(Get-HostConfigCandidates -App $App -ProfilePath $profiles[0])
    return $candidates[0]
}

function Get-CodexConfigPath {
    foreach ($profilePath in Get-CandidateProfiles) {
        $candidate = Join-Path $profilePath '.codex\config.toml'
        if (Test-Path -LiteralPath $candidate) { return $candidate }
    }
    return ''
}

# ---------------------------------------------------------------------------
# Host application detection.
# ---------------------------------------------------------------------------

function Test-HostInstalled {
    param([string]$App)

    $markers = @()
    foreach ($profilePath in Get-CandidateProfiles) {
        $local = Join-Path $profilePath 'AppData\Local'
        $roaming = Join-Path $profilePath 'AppData\Roaming'
        switch ($App) {
            'claude' {
                $markers += @(
                    (Join-Path $local 'AnthropicClaude'),
                    (Join-Path $local 'Programs\Claude'),
                    (Join-Path $roaming 'Claude')
                )
            }
            'chatgpt' {
                $markers += @(
                    (Join-Path $local 'Programs\ChatGPT'),
                    (Join-Path $local 'OpenAI\ChatGPT'),
                    (Join-Path $roaming 'ChatGPT'),
                    (Join-Path $profilePath '.codex')
                )
                $markers += @(Get-ChildItem -LiteralPath (Join-Path $local 'Packages') -Directory -Filter 'OpenAI.ChatGPT*' -ErrorAction SilentlyContinue |
                    ForEach-Object { $_.FullName })
            }
        }
    }

    switch ($App) {
        'claude' {
            $markers += @(
                (Join-Path ${env:ProgramFiles} 'Claude'),
                (Join-Path ${env:ProgramFiles(x86)} 'Claude')
            )
        }
        'chatgpt' {
            $markers += @(
                (Join-Path ${env:ProgramFiles} 'OpenAI\ChatGPT'),
                (Join-Path ${env:ProgramFiles} 'ChatGPT')
            )
        }
    }

    foreach ($marker in $markers) {
        if ($marker -and (Test-Path -LiteralPath $marker)) { return $marker }
    }
    return ''
}

function Find-SefimDirectory {
    $candidates = @(
        (Join-Path ${env:ProgramFiles(x86)} 'Vega\Sefim'),
        (Join-Path ${env:ProgramFiles} 'Vega\Sefim'),
        'C:\Program Files (x86)\Vega\Sefim',
        'C:\Vega\Sefim',
        'D:\Vega\Sefim'
    )
    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path -LiteralPath $candidate)) { return $candidate }
    }
    return ''
}

function Find-ProImages {
    param([string]$Root)
    if (-not $Root -or -not (Test-Path -LiteralPath $Root)) { return '' }

    $direct = Join-Path $Root 'proimages'
    if (Test-Path -LiteralPath $direct) { return (Get-Item -LiteralPath $direct).FullName }

    $parent = Split-Path -Parent $Root
    if ($parent) {
        $sibling = Join-Path $parent 'proimages'
        if (Test-Path -LiteralPath $sibling) { return (Get-Item -LiteralPath $sibling).FullName }
    }

    $nested = Get-ChildItem -LiteralPath $Root -Directory -Filter 'proimages' -Recurse -Depth 2 -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($nested) { return $nested.FullName }
    return ''
}

# ---------------------------------------------------------------------------
# connectionstring.txt parsing.
#
# Only the four fields the operator cares about are taken from the Sefim file;
# pooling, encryption and timeout values are always written by this installer.
# ---------------------------------------------------------------------------

function Get-ConnectionField {
    param([string]$Text, [string[]]$Names)

    foreach ($name in $Names) {
        $pattern = '(?im)(?:^|[;\r\n"''<>])\s*' + [regex]::Escape($name) + '\s*=\s*([^;\r\n"'']*)'
        $match = [regex]::Match($Text, $pattern)
        if ($match.Success) {
            $value = $match.Groups[1].Value.Trim()
            if ($value) { return $value }
        }
    }
    return ''
}

function Read-SefimConnectionString {
    param([string]$Root)

    $result = [ordered]@{ file = ''; server = ''; database = ''; userid = ''; password = '' }
    if (-not $Root -or -not (Test-Path -LiteralPath $Root)) { return $result }

    $file = Get-ChildItem -LiteralPath $Root -Filter 'connectionstring.txt' -File -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if (-not $file) {
        $file = Get-ChildItem -LiteralPath $Root -Filter 'connectionstring*.txt' -File -Recurse -Depth 2 -ErrorAction SilentlyContinue |
            Select-Object -First 1
    }
    if (-not $file) { return $result }

    $text = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
    $result.file = $file.FullName
    $result.server = Get-ConnectionField $text @('Data Source', 'Server', 'Address', 'Addr', 'Network Address')
    $result.database = Get-ConnectionField $text @('Initial Catalog', 'Database')
    $result.userid = Get-ConnectionField $text @('User ID', 'User Id', 'UserId', 'Uid')
    $result.password = Get-ConnectionField $text @('Password', 'Pwd')
    return $result
}

function New-SefimConnectionString {
    param([string]$Server, [string]$Database, [string]$UserId, [string]$Password)

    return "Server=$Server;Database=$Database;User Id=$UserId;Password=$Password;" +
           'TrustServerCertificate=True;Encrypt=True;Connection Timeout=30;Max Pool Size=100;' +
           'Min Pool Size=10;MultipleActiveResultSets=True;Application Name=VeposTransferCenterAPI;Language=Turkish;'
}

# ---------------------------------------------------------------------------
# JSON helpers.
# ---------------------------------------------------------------------------

function Read-JsonFile {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) { return (New-Object PSObject) }

    $raw = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    if (-not $raw -or -not $raw.Trim()) { return (New-Object PSObject) }

    try {
        $parsed = $raw | ConvertFrom-Json
    }
    catch {
        Fail "Existing JSON is not valid and was left untouched: $Path"
    }
    if ($null -eq $parsed) { return (New-Object PSObject) }
    return $parsed
}

function Save-JsonFile {
    param([string]$Path, $Object)

    $directory = Split-Path -Parent $Path
    if ($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    if (Test-Path -LiteralPath $Path) {
        $backup = "$Path.sefim-backup-" + (Get-Date -Format 'yyyyMMddHHmmss')
        Copy-Item -LiteralPath $Path -Destination $backup -Force
        Write-Pair 'backup' $backup
    }

    $json = $Object | ConvertTo-Json -Depth 100
    [System.IO.File]::WriteAllText($Path, $json, (New-Object System.Text.UTF8Encoding($false)))
}

function Set-JsonProperty {
    param($Object, [string]$Name, $Value)

    # The indexer is used instead of .Properties.Name: under Set-StrictMode the
    # latter throws on an object that has no properties at all.
    if ($null -ne $Object.PSObject.Properties[$Name]) {
        $Object.PSObject.Properties[$Name].Value = $Value
    }
    else {
        $Object | Add-Member -MemberType NoteProperty -Name $Name -Value $Value
    }
}

function Get-EntryCommand {
    param($Entry)

    if ($Entry -isnot [PSCustomObject]) { return '' }
    $property = $Entry.PSObject.Properties['command']
    if ($null -eq $property) { return '' }
    return [string]$property.Value
}

function Get-OrCreateSection {
    param($Object, [string]$Name)

    $existing = $Object.PSObject.Properties[$Name]
    if ($null -ne $existing -and $existing.Value -is [PSCustomObject]) {
        return $existing.Value
    }
    $section = New-Object PSObject
    Set-JsonProperty -Object $Object -Name $Name -Value $section
    return $section
}

# ---------------------------------------------------------------------------
# appsettings.json.
#
# The file shipped with the build is the template: it is read as-is and only the
# connection string and the image location are replaced, so logging, batch sizes
# and any hand-made edit survive an upgrade.
# ---------------------------------------------------------------------------

function Update-AppSettings {
    param([string]$Path, [string]$ConnectionString, [string]$ImageLocation)

    $settings = Read-JsonFile -Path $Path
    $sql = Get-OrCreateSection -Object $settings -Name 'SqlService'
    Set-JsonProperty -Object $sql -Name 'ConnectionString' -Value $ConnectionString

    if ($ImageLocation) {
        $sefim = Get-OrCreateSection -Object $settings -Name 'SEFIM'
        Set-JsonProperty -Object $sefim -Name 'ImageLocation' -Value $ImageLocation
    }

    Save-JsonFile -Path $Path -Object $settings
}

# ---------------------------------------------------------------------------
# MCP host configuration.
# ---------------------------------------------------------------------------

function Get-ServerEntry {
    param([string]$ExePath, [string]$ToolProfile, [string]$KnowledgeKey)

    $environment = New-Object PSObject
    Set-JsonProperty -Object $environment -Name 'SEFIM_TOOL_PROFILE' -Value $ToolProfile
    if ($KnowledgeKey) {
        Set-JsonProperty -Object $environment -Name 'SEFIM_KNOWLEDGE_KEY' -Value $KnowledgeKey
    }

    # "args" is omitted on purpose: the server takes no command line arguments, and an
    # empty array is the one value ConvertTo-Json in PowerShell 5.1 is unreliable about.
    $entry = New-Object PSObject
    Set-JsonProperty -Object $entry -Name 'command' -Value $ExePath
    Set-JsonProperty -Object $entry -Name 'env' -Value $environment
    return $entry
}

function Update-HostConfig {
    param([string]$Path, [string]$Key, [string]$ExePath, [string]$ToolProfile, [string]$KnowledgeKey)

    $config = Read-JsonFile -Path $Path
    $servers = Get-OrCreateSection -Object $config -Name 'mcpServers'

    # An older release lives in a different versioned folder. Its entry is dropped
    # so the host does not keep launching a stale server next to the new one.
    foreach ($property in @($servers.PSObject.Properties)) {
        if ($property.Name -eq $Key) { continue }
        $command = Get-EntryCommand -Entry $property.Value
        if ($command -and $command -match '(?i)SEFIM-MCP|sefim-ai-mcp\.exe') {
            $servers.PSObject.Properties.Remove($property.Name)
            Write-Pair 'removed' $property.Name
        }
    }

    Set-JsonProperty -Object $servers -Name $Key -Value (Get-ServerEntry -ExePath $ExePath -ToolProfile $ToolProfile -KnowledgeKey $KnowledgeKey)
    Save-JsonFile -Path $Path -Object $config
}

function Remove-HostConfigEntry {
    param([string]$Path, [string]$Key)

    if (-not (Test-Path -LiteralPath $Path)) { return $false }

    $config = Read-JsonFile -Path $Path
    $section = $config.PSObject.Properties['mcpServers']
    if ($null -eq $section) { return $false }

    $servers = $section.Value
    if ($servers -isnot [PSCustomObject]) { return $false }

    $changed = $false
    foreach ($property in @($servers.PSObject.Properties)) {
        $command = Get-EntryCommand -Entry $property.Value
        if ($property.Name -eq $Key -or ($command -and $command -match '(?i)SEFIM-MCP|sefim-ai-mcp\.exe')) {
            $servers.PSObject.Properties.Remove($property.Name)
            $changed = $true
        }
    }

    if ($changed) { Save-JsonFile -Path $Path -Object $config }
    return $changed
}

# ---------------------------------------------------------------------------
# Codex (OpenAI CLI) TOML support. Only touched when the file already exists.
# ---------------------------------------------------------------------------

function Update-CodexConfig {
    param([string]$Path, [string]$Key, [string]$ExePath, [string]$ToolProfile, [string]$KnowledgeKey, [switch]$Remove)

    if (-not (Test-Path -LiteralPath $Path)) { return }

    $text = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $backup = "$Path.sefim-backup-" + (Get-Date -Format 'yyyyMMddHHmmss')
    Copy-Item -LiteralPath $Path -Destination $backup -Force

    # Drop every previously written Sefim section, including older version folders.
    $pattern = '(?ms)^\[mcp_servers\.' + [regex]::Escape($Key) + '\].*?(?=^\[|\z)'
    $text = [regex]::Replace($text, $pattern, '')

    if (-not $Remove) {
        $escapedExe = $ExePath -replace '\\', '\\\\'
        $block = "[mcp_servers.$Key]`r`n" +
                 "command = `"$escapedExe`"`r`n" +
                 "args = []`r`n" +
                 "env = { SEFIM_TOOL_PROFILE = `"$ToolProfile`""
        if ($KnowledgeKey) { $block += ", SEFIM_KNOWLEDGE_KEY = `"$KnowledgeKey`"" }
        $block += " }`r`n"
        $text = $text.TrimEnd() + "`r`n`r`n" + $block
    }

    [System.IO.File]::WriteAllText($Path, $text.TrimStart(), (New-Object System.Text.UTF8Encoding($false)))
    Write-Pair 'codexconfig' $Path
}

# ---------------------------------------------------------------------------
# Modes.
# ---------------------------------------------------------------------------

function Invoke-Detect {
    $claude = Test-HostInstalled -App 'claude'
    $chatgpt = Test-HostInstalled -App 'chatgpt'
    Write-Pair 'claude' ($(if ($claude) { '1' } else { '0' }))
    Write-Pair 'claudepath' $claude
    Write-Pair 'chatgpt' ($(if ($chatgpt) { '1' } else { '0' }))
    Write-Pair 'chatgptpath' $chatgpt

    $sefim = Find-SefimDirectory
    Write-Pair 'sefimdir' $sefim
    Write-Pair 'proimages' (Find-ProImages -Root $sefim)

    $parsed = Read-SefimConnectionString -Root $sefim
    Write-Pair 'csfile' $parsed.file
    Write-Pair 'server' $parsed.server
    Write-Pair 'database' $parsed.database
    Write-Pair 'userid' $parsed.userid
    Write-Pair 'password' $parsed.password
    Write-Pair 'status' 'ok'
}

function Invoke-Parse {
    $parsed = Read-SefimConnectionString -Root $SefimDir
    Write-Pair 'csfile' $parsed.file
    Write-Pair 'server' $parsed.server
    Write-Pair 'database' $parsed.database
    Write-Pair 'userid' $parsed.userid
    Write-Pair 'password' $parsed.password
    Write-Pair 'proimages' (Find-ProImages -Root $SefimDir)
    Write-Pair 'status' 'ok'
}

function Invoke-Test {
    if (-not $Server -or -not $Database) { Fail 'Server and database are required.' }

    $connectionString = "Server=$Server;Database=$Database;User Id=$UserId;Password=$Password;" +
                        'TrustServerCertificate=True;Encrypt=True;Connection Timeout=8;Application Name=SefimMcpSetup;'
    $connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = 'SELECT DB_NAME()'
        $name = [string]$command.ExecuteScalar()
        Write-Pair 'status' 'ok'
        Write-Pair 'message' "Connected to $name on $Server."
    }
    catch {
        Fail $_.Exception.Message
    }
    finally {
        $connection.Dispose()
    }
}

function Invoke-Configure {
    if (-not $InstallDir) { Fail 'InstallDir is required.' }
    if (-not $HostApp) { Fail 'HostApp is required.' }

    $exePath = Join-Path $InstallDir $ExeName
    if (-not (Test-Path -LiteralPath $exePath)) { Fail "Server executable not found: $exePath" }

    $connectionString = New-SefimConnectionString -Server $Server -Database $Database -UserId $UserId -Password $Password
    $imageLocation = Find-ProImages -Root $SefimDir

    Update-AppSettings -Path (Join-Path $InstallDir 'appsettings.json') `
                       -ConnectionString $connectionString `
                       -ImageLocation $imageLocation
    Write-Pair 'appsettings' (Join-Path $InstallDir 'appsettings.json')
    Write-Pair 'imagelocation' $imageLocation

    $configPath = Resolve-HostConfigPath -App $HostApp
    Update-HostConfig -Path $configPath -Key $ServerKey -ExePath $exePath -ToolProfile $ToolProfile -KnowledgeKey $KnowledgeKey
    Write-Pair 'hostconfig' $configPath

    if ($HostApp -eq 'chatgpt') {
        $codex = Get-CodexConfigPath
        if ($codex) {
            Update-CodexConfig -Path $codex -Key $ServerKey -ExePath $exePath -ToolProfile $ToolProfile -KnowledgeKey $KnowledgeKey
        }
    }

    Write-Pair 'status' 'ok'
}

function Invoke-Remove {
    $apps = if ($HostApp) { @($HostApp) } else { @('claude', 'chatgpt') }
    foreach ($app in $apps) {
        $configPath = Resolve-HostConfigPath -App $app -MustExist
        if ($configPath -and (Remove-HostConfigEntry -Path $configPath -Key $ServerKey)) {
            Write-Pair 'cleaned' $configPath
        }
    }

    $codex = Get-CodexConfigPath
    if ($codex) {
        Update-CodexConfig -Path $codex -Key $ServerKey -ExePath '' -ToolProfile $ToolProfile -KnowledgeKey '' -Remove
    }

    Write-Pair 'status' 'ok'
}

try {
    Import-InputFile -Path $InputFile

    switch ($Mode) {
        'Detect' { Invoke-Detect }
        'Parse' { Invoke-Parse }
        'Test' { Invoke-Test }
        'Configure' { Invoke-Configure }
        'Remove' { Invoke-Remove }
    }
    Save-Result
    exit 0
}
catch {
    Write-Pair 'status' 'error'
    Write-Pair 'message' $_.Exception.Message
    Save-Result
    exit 1
}
