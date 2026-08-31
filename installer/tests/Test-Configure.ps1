<#
.SYNOPSIS
    Verifies scripts\Configure-SefimMcp.ps1 against a synthetic installation.

.DESCRIPTION
    Runs on Windows PowerShell 5.1 (the version the installer uses on the target
    machine) and asserts the three behaviours the setup depends on:

      * connectionstring.txt is reduced to Data Source / Initial Catalog /
        User ID / Password and rebuilt into the fixed Sefim connection string
      * appsettings.json keeps every unrelated setting
      * the MCP host configuration keeps every unrelated key and server, while a
        stale entry from an older version folder is replaced

    Exit code 0 means all assertions passed.
#>

[CmdletBinding()]
param(
    [string]$ScriptPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'scripts\Configure-SefimMcp.ps1')
)

$ErrorActionPreference = 'Stop'

$script:Failures = 0

function Assert-Equal {
    param([string]$Name, $Expected, $Actual)
    if ([string]$Expected -eq [string]$Actual) {
        Write-Host "PASS  $Name"
    }
    else {
        Write-Host "FAIL  $Name"
        Write-Host "      expected: $Expected"
        Write-Host "      actual  : $Actual"
        $script:Failures++
    }
}

function Assert-True {
    param([string]$Name, [bool]$Condition)
    if ($Condition) { Write-Host "PASS  $Name" }
    else {
        Write-Host "FAIL  $Name"
        $script:Failures++
    }
}

# ---------------------------------------------------------------------------
# Synthetic machine
# ---------------------------------------------------------------------------

$root = Join-Path $env:TEMP ("sefim-installer-test-" + [Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $root -Force | Out-Null

# TEMP can be an 8.3 short path (C:\Users\RUNNER~1\...) while the helper reports the
# expanded name, so the expected values are built from the expanded form as well.
$root = (Get-Item -LiteralPath $root).FullName

$installDir = Join-Path $root 'Program Files\OZFILIZYAZILIM\SEFIM-MCP\1.0.0'
$oldInstallDir = Join-Path $root 'Program Files\OZFILIZYAZILIM\SEFIM-MCP\0.9.0'
$sefimDir = Join-Path $root 'Vega\Sefim'

New-Item -ItemType Directory -Path $installDir -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $sefimDir 'proimages') -Force | Out-Null

Set-Content -LiteralPath (Join-Path $installDir 'sefim-ai-mcp.exe') -Value 'stub' -Encoding ASCII

@'
{
    "Logging": {
        "LogLevel": {
            "Default": "Information"
        }
    },
    "SqlService": {
        "ConnectionString": "",
        "CommandTimeout": 300,
        "EnableLogging": false
    },
    "SEFIM": {
        "ImageLocation": "/mnt/proimages"
    }
}
'@ | Set-Content -LiteralPath (Join-Path $installDir 'appsettings.json') -Encoding UTF8

@'
<connectionStrings>
  <add name="Vega" connectionString="Data Source=SRV-POS\SQLEXPRESS;Initial Catalog=veposmerkez;User ID=sa;Password=123456a.A;Persist Security Info=True;MultipleActiveResultSets=True" />
</connectionStrings>
'@ | Set-Content -LiteralPath (Join-Path $sefimDir 'connectionstring.txt') -Encoding UTF8

# A host configuration that already contains foreign data and an older Sefim entry.
$hostConfigPath = Join-Path $env:APPDATA 'Claude\claude_desktop_config.json'
$hostConfigDir = Split-Path -Parent $hostConfigPath
if (-not (Test-Path -LiteralPath $hostConfigDir)) {
    New-Item -ItemType Directory -Path $hostConfigDir -Force | Out-Null
}

$restoreConfig = $null
if (Test-Path -LiteralPath $hostConfigPath) {
    $restoreConfig = Get-Content -LiteralPath $hostConfigPath -Raw -Encoding UTF8
}

$oldExe = (Join-Path $oldInstallDir 'sefim-ai-mcp.exe')
@"
{
  "globalShortcut": "Ctrl+Space",
  "theme": "dark",
  "mcpServers": {
    "filesystem": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-filesystem", "C:\\\\data"]
    },
    "sefim-old": {
      "command": "$($oldExe -replace '\\', '\\\\')",
      "env": { "SEFIM_TOOL_PROFILE": "core" }
    }
  }
}
"@ | Set-Content -LiteralPath $hostConfigPath -Encoding UTF8

try {
    # -----------------------------------------------------------------------
    # Parse
    # -----------------------------------------------------------------------
    $parse = & $ScriptPath -Mode Parse -SefimDir $sefimDir
    $parsed = @{}
    foreach ($line in $parse) {
        $index = $line.IndexOf('=')
        if ($index -gt 0) { $parsed[$line.Substring(0, $index)] = $line.Substring($index + 1) }
    }

    Assert-Equal 'parse server' 'SRV-POS\SQLEXPRESS' $parsed['server']
    Assert-Equal 'parse database' 'veposmerkez' $parsed['database']
    Assert-Equal 'parse user' 'sa' $parsed['userid']
    Assert-Equal 'parse password' '123456a.A' $parsed['password']
    Assert-Equal 'parse proimages' (Join-Path $sefimDir 'proimages') $parsed['proimages']

    # -----------------------------------------------------------------------
    # Configure
    # -----------------------------------------------------------------------
    & $ScriptPath -Mode Configure `
        -InstallDir $installDir `
        -HostApp claude `
        -SefimDir $sefimDir `
        -Server $parsed['server'] `
        -Database $parsed['database'] `
        -UserId $parsed['userid'] `
        -Password $parsed['password'] | Out-Null

    $settings = Get-Content -LiteralPath (Join-Path $installDir 'appsettings.json') -Raw -Encoding UTF8 | ConvertFrom-Json

    $expected = 'Server=SRV-POS\SQLEXPRESS;Database=veposmerkez;User Id=sa;Password=123456a.A;' +
                'TrustServerCertificate=True;Encrypt=True;Connection Timeout=30;Max Pool Size=100;' +
                'Min Pool Size=10;MultipleActiveResultSets=True;Application Name=VeposTransferCenterAPI;Language=Turkish;'

    Assert-Equal 'appsettings connection string' $expected $settings.SqlService.ConnectionString
    Assert-Equal 'appsettings image location' (Join-Path $sefimDir 'proimages') $settings.SEFIM.ImageLocation
    Assert-Equal 'appsettings keeps CommandTimeout' 300 $settings.SqlService.CommandTimeout
    Assert-Equal 'appsettings keeps logging' 'Information' $settings.Logging.LogLevel.Default

    $config = Get-Content -LiteralPath $hostConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json

    Assert-Equal 'host config keeps unrelated key' 'dark' $config.theme
    Assert-Equal 'host config keeps unrelated shortcut' 'Ctrl+Space' $config.globalShortcut
    Assert-Equal 'host config keeps other server' 'npx' $config.mcpServers.filesystem.command
    Assert-True 'host config keeps other server args' ($config.mcpServers.filesystem.args.Count -eq 3)
    Assert-Equal 'host config points at new version' (Join-Path $installDir 'sefim-ai-mcp.exe') $config.mcpServers.sefim.command
    Assert-Equal 'host config tool profile' 'full' $config.mcpServers.sefim.env.SEFIM_TOOL_PROFILE
    Assert-True 'host config drops stale version entry' ($config.mcpServers.PSObject.Properties.Name -notcontains 'sefim-old')

    # -----------------------------------------------------------------------
    # Remove
    # -----------------------------------------------------------------------
    & $ScriptPath -Mode Remove -HostApp claude | Out-Null

    $config = Get-Content -LiteralPath $hostConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-True 'uninstall removes sefim entry' ($config.mcpServers.PSObject.Properties.Name -notcontains 'sefim')
    Assert-Equal 'uninstall keeps other server' 'npx' $config.mcpServers.filesystem.command
    Assert-Equal 'uninstall keeps unrelated key' 'dark' $config.theme
}
finally {
    if ($restoreConfig) {
        Set-Content -LiteralPath $hostConfigPath -Value $restoreConfig -Encoding UTF8 -NoNewline
    }
    else {
        Remove-Item -LiteralPath $hostConfigPath -Force -ErrorAction SilentlyContinue
    }
    Get-ChildItem -LiteralPath $hostConfigDir -Filter '*.sefim-backup-*' -ErrorAction SilentlyContinue |
        Remove-Item -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
}

if ($script:Failures -gt 0) {
    Write-Host ""
    Write-Host "$($script:Failures) assertion(s) failed."
    exit 1
}

Write-Host ""
Write-Host 'All installer configuration assertions passed.'
exit 0
