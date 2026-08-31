# Cross-Platform QtIFW Installer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the Windows-only NSIS setup with a single Qt Installer Framework (QtIFW) based wizard that builds and runs on Windows, Linux and macOS, ships full feature parity with the old NSIS flow (client selection, Sefim/database detection, appsettings.json + MCP client config writing, uninstall), adds a dedicated selectable images-path field with OS-specific defaults, makes the SQL user/password optional, and is produced by a production-ready GitHub Actions matrix that builds native AOT payloads for win-x64/arm64, linux-x64/arm64 and osx-x64/arm64, then attaches every installer to the GitHub Release.

**Architecture:**
The PowerShell configuration helper (`Configure-SefimMcp.ps1`) is Windows-only and cannot be reused on Linux/macOS. Its logic (Detect / Parse / Test / Configure / Remove) is ported into the existing `sefim-ai-mcp` binary itself as a new `setup` CLI subcommand, following the exact pattern the codebase already uses for `knowledge` ([sefim-ai-mcp/Program.cs](../../../sefim-ai-mcp/Program.cs), [sefim-ai-mcp/Knowledge/Authoring/KnowledgeCommandRunner.cs](../../../sefim-ai-mcp/Knowledge/Authoring/KnowledgeCommandRunner.cs)). Because this logic now lives in the already-per-RID-published, AOT-compiled server executable, the QtIFW wizard never needs a separate helper binary or PowerShell — it just runs `<payload>/sefim-ai-mcp[.exe] setup <mode> ...` as ordered install-time operations, immediately after the default file-extraction operation, using the QtIFW `@TargetDir@` macro (confirmed via Qt Installer Framework docs: `Component.prototype.createOperations` + `component.addOperation("Execute", ...)`). The custom wizard page (`SefimPage.ui`) is pure Qt Designer UI + JS field validation/browsing (`QFileDialog`, confirmed global scripting object) — it collects input only, it does not execute any process pre-install, which avoids the one genuinely unresolved QtIFW-scripting question (whether meta-embedded binaries get a real pre-install filesystem path). Detect/Test/Configure all run automatically during the "Perform Installation" page and their output appears in the install log, exactly like the old NSIS `DetailPrint` output; failures are non-fatal warnings, matching current NSIS behavior.

Install scope is **per-user** (confirmed with user): `%LOCALAPPDATA%` on Windows, `~/.local/share` on Linux, `~/Library/Application Support` on macOS — no admin/sudo/pkexec elevation anywhere.

**Tech Stack:** .NET 10 (native AOT, existing project), Qt Installer Framework (`binarycreator`, installed via `aqtinstall`), xunit (existing test project), GitHub Actions.

## Global Constraints

- Per-user install directories only (no elevation) — confirmed decision, applies to every OS.
- `Server` and `Database` remain required; `UserId`/`Password` are optional (blank ⇒ `Integrated Security=True` instead of `User Id=`/`Password=`).
- Images (`proimages`) path is a dedicated, always-editable, always-browsable field with an OS-specific default guess — never silently derived only from the Sefim dir.
- No plaintext knowledge (`*.md`) may ever ship in a payload; the encrypted `knowledge.pack` (`SEFIMKP1` header) is the only allowed business-knowledge artifact in a release build — this check already exists for Windows and must be replicated for every OS job.
- Native AOT cannot cross-compile across OS families, so every RID is built on a runner of its own OS/arch (no Linux→Windows or x64→arm64-emulated cross builds): `windows-latest` (win-x64, win-arm64 — same-OS cross-arch AOT publish is supported), `ubuntu-latest` (linux-x64), `ubuntu-24.04-arm` (linux-arm64), `macos-13` (osx-x64, Intel), `macos-14` (osx-arm64, Apple Silicon).
- Confirmed QtIFW scripting facts to rely on (from Qt Installer Framework docs, retrieved via Context7 — do not re-derive from memory): `installer.value("InstallerDirPath")`, `installer.value("TargetDir")`, `installer.value("os")` / `systemInfo`, `installer.addWizardPage(component, "WidgetName", QInstaller.TargetDirectory)`, dynamic page callbacks `Controller.prototype.<ObjectName>Callback`, `component.addOperation("Execute", [...])`, `@TargetDir@` macro expansion inside operation arguments, global `QFileDialog` object. Anything beyond this list that a task needs (e.g. exact `systemInfo` property names) must be verified with Context7 (`resolve-library-id` → `Qt`, then `query-docs` against `/websites/doc_qt_io`) before writing code — do not guess.
- Every deleted-and-replaced file must actually be deleted with `git rm`, not left behind dead.

---

## File Structure

```
sefim-ai-mcp/
  Program.cs                                  [MODIFY] wire in SetupCommandRunner
  Setup/
    SetupCommandRunner.cs                     [CREATE] CLI dispatch (detect|parse|test|configure|remove)
    SefimDetection.cs                         [CREATE] Sefim dir / connectionstring.txt / proimages discovery, OS-aware
    ClientConfigWriter.cs                     [CREATE] Claude/ChatGPT/Codex config file read-merge-write
    AppSettingsWriter.cs                      [CREATE] appsettings.json ConnectionString/ImageLocation writer
    SqlConnectionTester.cs                    [CREATE] SQL Server connectivity probe
    SetupPaths.cs                             [CREATE] per-OS path/profile helpers
  sefim-ai-mcp.csproj                         [MODIFY] add osx-x64 RID

tests/SefimMcp.Tests/
  SetupCommandRunnerTests.cs                  [CREATE]
  SefimDetectionTests.cs                      [CREATE]
  ClientConfigWriterTests.cs                  [CREATE]

installer/                                    [DELETE former Windows-only tree, see Task 3]
  qtifw/
    config/
      config.xml                              [CREATE]
    packages/
      com.ozfiliz.sefimmcp/
        meta/
          package.xml                         [CREATE]
          installscript.qs                    [CREATE]
          ClientPage.ui                        [CREATE]
          SefimPage.ui                          [CREATE]
        data/                                  [gitignored — populated at build time from published payload]
    assets/
      generate-assets.py                      [CREATE, replaces installer/assets/generate-assets.py]
    build-installer.sh                        [CREATE]
    README.md                                 [CREATE]

.github/workflows/
  windows-installer.yml                       [DELETE]
  release.yml                                 [CREATE]
```

---

### Task 1: Add `osx-x64` runtime identifier

**Files:**
- Modify: `sefim-ai-mcp/sefim-ai-mcp.csproj:8`

**Interfaces:** none (build config only).

- [ ] **Step 1: Edit the RuntimeIdentifiers list**

```xml
<RuntimeIdentifiers>win-x64;win-arm64;osx-x64;osx-arm64;linux-x64;linux-arm64;linux-musl-x64</RuntimeIdentifiers>
```

- [ ] **Step 2: Verify it resolves**

Run: `dotnet restore sefim-ai-mcp.sln`
Expected: restore succeeds, no errors about `osx-x64`.

- [ ] **Step 3: Commit**

```bash
git add sefim-ai-mcp/sefim-ai-mcp.csproj
git commit -m "build: add osx-x64 runtime identifier for Intel Mac installer builds"
```

---

### Task 2: `SetupPaths` — per-OS path/profile helpers

**Files:**
- Create: `sefim-ai-mcp/Setup/SetupPaths.cs`
- Test: `tests/SefimMcp.Tests/SetupPathsTests.cs`

**Interfaces:**
- Produces: `static class SetupPaths` with:
  - `static string InstallRoot(string productDir, string version)` — per-user base install dir for current OS.
  - `static IReadOnlyList<string> SefimDirCandidates()` — OS-aware Sefim install-dir guesses.
  - `static IReadOnlyList<string> ProImagesCandidates(string? sefimDir)` — OS-aware image-folder guesses (includes the Linux mount points implied by the checked-in `appsettings.json` default `/mnt/proimages`, plus `/Volumes/...` on macOS).
  - `static string? ClaudeConfigPath()` / `static string? ChatGptConfigPath()` / `static string? CodexConfigPath()` — per-OS, per-user client config file paths (only paths, existence not required).

- [ ] **Step 1: Write the failing tests**

```csharp
using SefimMcp.Setup;
using Xunit;

namespace SefimMcp.Tests;

public class SetupPathsTests
{
    [Fact]
    public void InstallRoot_IsUnderUserProfile_NotProgramFiles()
    {
        var root = SetupPaths.InstallRoot("SEFIM-MCP", "0.1.0-beta");

        Assert.DoesNotContain("Program Files", root);
        Assert.Contains("0.1.0-beta", root);
    }

    [Fact]
    public void ProImagesCandidates_IncludesMntProimages_OnNonWindows()
    {
        var candidates = SetupPaths.ProImagesCandidates(sefimDir: null);

        if (!OperatingSystem.IsWindows())
            Assert.Contains("/mnt/proimages", candidates);
    }

    [Fact]
    public void SefimDirCandidates_IncludesVegaSefim_OnWindows()
    {
        var candidates = SetupPaths.SefimDirCandidates();

        if (OperatingSystem.IsWindows())
            Assert.Contains(candidates, c => c.Contains("Vega") && c.Contains("Sefim"));
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test tests/SefimMcp.Tests/SefimMcp.Tests.csproj --filter SetupPathsTests`
Expected: FAIL — `SefimMcp.Setup` namespace / `SetupPaths` type does not exist.

- [ ] **Step 3: Implement**

```csharp
namespace SefimMcp.Setup;

/// <summary>Per-OS default paths. Every value here is a guess to pre-fill a UI field with —
/// never treated as authoritative without an existence check by the caller.</summary>
public static class SetupPaths
{
    public static string InstallRoot(string productDir, string version)
    {
        var baseDir = OperatingSystem.IsWindows()
            ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            : OperatingSystem.IsMacOS()
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

        return Path.Combine(baseDir, "OZFILIZYAZILIM", productDir, version);
    }

    public static IReadOnlyList<string> SefimDirCandidates()
    {
        if (OperatingSystem.IsWindows())
        {
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            return new[]
            {
                Path.Combine(programFilesX86, "Vega", "Sefim"),
                Path.Combine(programFiles, "Vega", "Sefim"),
                @"C:\Program Files (x86)\Vega\Sefim",
                @"C:\Vega\Sefim",
                @"D:\Vega\Sefim",
            };
        }

        // Sefim itself is Windows-only POS software; on Linux/macOS the MCP server reaches it
        // over a network share, so there is no local install directory to guess reliably.
        return Array.Empty<string>();
    }

    public static IReadOnlyList<string> ProImagesCandidates(string? sefimDir)
    {
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(sefimDir))
        {
            candidates.Add(Path.Combine(sefimDir, "proimages"));
            var parent = Path.GetDirectoryName(sefimDir);
            if (!string.IsNullOrEmpty(parent))
                candidates.Add(Path.Combine(parent, "proimages"));
        }

        if (OperatingSystem.IsWindows())
        {
            candidates.Add(@"C:\Program Files (x86)\Vega\Sefim\proimages");
        }
        else if (OperatingSystem.IsMacOS())
        {
            candidates.Add("/Volumes/Sefim/proimages");
            candidates.Add("/Volumes/proimages");
        }
        else
        {
            // Matches the default already checked into sefim-ai-mcp/appsettings.json.
            candidates.Add("/mnt/proimages");
            candidates.Add("/mnt/sefim/proimages");
            candidates.Add("/media/sefim/proimages");
            candidates.Add("/srv/sefim/proimages");
        }

        return candidates;
    }

    public static string? ClaudeConfigPath()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Claude", "claude_desktop_config.json");
        if (OperatingSystem.IsMacOS())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "Claude", "claude_desktop_config.json");
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "Claude", "claude_desktop_config.json");
    }

    public static string? ChatGptConfigPath()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChatGPT", "mcp_config.json");
        if (OperatingSystem.IsMacOS())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "ChatGPT", "mcp_config.json");
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "ChatGPT", "mcp_config.json");
    }

    public static string CodexConfigPath()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codex", "config.toml");
}
```

- [ ] **Step 4: Run tests, verify pass**

Run: `dotnet test tests/SefimMcp.Tests/SefimMcp.Tests.csproj --filter SetupPathsTests`
Expected: PASS (3/3).

- [ ] **Step 5: Commit**

```bash
git add sefim-ai-mcp/Setup/SetupPaths.cs tests/SefimMcp.Tests/SetupPathsTests.cs
git commit -m "feat(setup): add per-OS install/detection path helpers"
```

---

### Task 3: `SefimDetection` — connectionstring.txt parsing + directory scan

**Files:**
- Create: `sefim-ai-mcp/Setup/SefimDetection.cs`
- Test: `tests/SefimMcp.Tests/SefimDetectionTests.cs`

**Interfaces:**
- Consumes: `SetupPaths.SefimDirCandidates()`, `SetupPaths.ProImagesCandidates(string?)` (Task 2).
- Produces:
  ```csharp
  public sealed record ConnectionFields(string? Server, string? Database, string? UserId, string? Password, string? SourceFile);
  public static class SefimDetection
  {
      public static string? FindSefimDirectory();
      public static string? FindProImages(string? sefimRoot);
      public static ConnectionFields ReadConnectionString(string? sefimRoot);
      public static string BuildConnectionString(string server, string database, string? userId, string? password);
  }
  ```
  `BuildConnectionString` is what later tasks (`AppSettingsWriter`, `SetupCommandRunner`) call; when `userId`/`password` are both null/blank it emits `Integrated Security=True` instead of `User Id=`/`Password=`.

- [ ] **Step 1: Write the failing tests**

```csharp
using SefimMcp.Setup;
using Xunit;

namespace SefimMcp.Tests;

public class SefimDetectionTests
{
    [Fact]
    public void BuildConnectionString_WithCredentials_IncludesUserAndPassword()
    {
        var cs = SefimDetection.BuildConnectionString("SQLHOST", "SefimDb", "sa", "P@ss");

        Assert.Contains("User Id=sa;", cs);
        Assert.Contains("Password=P@ss;", cs);
        Assert.DoesNotContain("Integrated Security", cs);
    }

    [Fact]
    public void BuildConnectionString_WithoutCredentials_UsesIntegratedSecurity()
    {
        var cs = SefimDetection.BuildConnectionString("SQLHOST", "SefimDb", null, null);

        Assert.Contains("Integrated Security=True;", cs);
        Assert.DoesNotContain("User Id=", cs);
        Assert.DoesNotContain("Password=", cs);
    }

    [Fact]
    public void ReadConnectionString_ParsesKnownFields()
    {
        var dir = Directory.CreateTempSubdirectory("sefim-detect-test");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "connectionstring.txt"),
                "Data Source=SQLHOST;Initial Catalog=SefimDb;User ID=sa;Password=P@ss;");

            var fields = SefimDetection.ReadConnectionString(dir.FullName);

            Assert.Equal("SQLHOST", fields.Server);
            Assert.Equal("SefimDb", fields.Database);
            Assert.Equal("sa", fields.UserId);
            Assert.Equal("P@ss", fields.Password);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void FindProImages_ReturnsDirectDirectory_WhenPresent()
    {
        var dir = Directory.CreateTempSubdirectory("sefim-detect-images");
        try
        {
            var images = Directory.CreateDirectory(Path.Combine(dir.FullName, "proimages"));

            var found = SefimDetection.FindProImages(dir.FullName);

            Assert.Equal(images.FullName, found);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test tests/SefimMcp.Tests/SefimMcp.Tests.csproj --filter SefimDetectionTests`
Expected: FAIL — `SefimDetection` does not exist.

- [ ] **Step 3: Implement**

```csharp
using System.Text.RegularExpressions;

namespace SefimMcp.Setup;

public sealed record ConnectionFields(string? Server, string? Database, string? UserId, string? Password, string? SourceFile);

public static class SefimDetection
{
    public static string? FindSefimDirectory()
        => SetupPaths.SefimDirCandidates().FirstOrDefault(Directory.Exists);

    public static string? FindProImages(string? sefimRoot)
    {
        foreach (var candidate in SetupPaths.ProImagesCandidates(sefimRoot))
        {
            if (Directory.Exists(candidate))
                return candidate;
        }

        if (!string.IsNullOrWhiteSpace(sefimRoot) && Directory.Exists(sefimRoot))
        {
            var nested = Directory.EnumerateDirectories(sefimRoot, "proimages", SearchOption.AllDirectories)
                .FirstOrDefault();
            if (nested is not null)
                return nested;
        }

        return null;
    }

    public static ConnectionFields ReadConnectionString(string? sefimRoot)
    {
        if (string.IsNullOrWhiteSpace(sefimRoot) || !Directory.Exists(sefimRoot))
            return new ConnectionFields(null, null, null, null, null);

        var file = Directory.EnumerateFiles(sefimRoot, "connectionstring*.txt", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (file is null)
            return new ConnectionFields(null, null, null, null, null);

        var text = File.ReadAllText(file);
        return new ConnectionFields(
            Server: GetField(text, "Data Source", "Server", "Address", "Addr", "Network Address"),
            Database: GetField(text, "Initial Catalog", "Database"),
            UserId: GetField(text, "User ID", "User Id", "UserId", "Uid"),
            Password: GetField(text, "Password", "Pwd"),
            SourceFile: file);
    }

    public static string BuildConnectionString(string server, string database, string? userId, string? password)
    {
        var auth = string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(password)
            ? "Integrated Security=True;"
            : $"User Id={userId};Password={password};";

        return $"Server={server};Database={database};{auth}" +
               "TrustServerCertificate=True;Encrypt=True;Connection Timeout=30;Max Pool Size=100;" +
               "Min Pool Size=10;MultipleActiveResultSets=True;Application Name=VeposTransferCenterAPI;Language=Turkish;";
    }

    private static string? GetField(string text, params string[] names)
    {
        foreach (var name in names)
        {
            var pattern = $@"(?im)(?:^|[;\r\n""'<>])\s*{Regex.Escape(name)}\s*=\s*([^;\r\n""']*)";
            var match = Regex.Match(text, pattern);
            if (match.Success)
            {
                var value = match.Groups[1].Value.Trim();
                if (value.Length > 0)
                    return value;
            }
        }
        return null;
    }
}
```

- [ ] **Step 4: Run tests, verify pass**

Run: `dotnet test tests/SefimMcp.Tests/SefimMcp.Tests.csproj --filter SefimDetectionTests`
Expected: PASS (4/4).

- [ ] **Step 5: Commit**

```bash
git add sefim-ai-mcp/Setup/SefimDetection.cs tests/SefimMcp.Tests/SefimDetectionTests.cs
git commit -m "feat(setup): port connectionstring.txt detection from PowerShell to cross-platform C#"
```

---

### Task 4: `AppSettingsWriter` and `ClientConfigWriter` — AOT-safe JSON merge

**Files:**
- Create: `sefim-ai-mcp/Setup/AppSettingsWriter.cs`
- Create: `sefim-ai-mcp/Setup/ClientConfigWriter.cs`
- Test: `tests/SefimMcp.Tests/ClientConfigWriterTests.cs`

**Interfaces:**
- Consumes: nothing from earlier tasks (pure JSON/file I/O). Uses `System.Text.Json.Nodes.JsonObject`/`JsonNode` — reflection-free, so it stays AOT/trim-safe under `PublishAot=true` / `PublishTrimmed=true` (unlike `JsonSerializer.Deserialize<T>` on an untyped shape, which the old PowerShell approach didn't need to worry about but this project does).
- Produces:
  ```csharp
  public static class AppSettingsWriter
  {
      public static void Update(string appSettingsPath, string connectionString, string? imageLocation);
  }

  public static class ClientConfigWriter
  {
      public static string UpdateJsonHost(string configPath, string serverKey, string exePath, string toolProfile);
      public static bool RemoveJsonHost(string configPath, string serverKey);
      public static void UpdateCodexToml(string configPath, string serverKey, string exePath, string toolProfile, bool remove);
  }
  ```
  Both writer classes back up an existing target file to `<path>.sefim-backup-<yyyyMMddHHmmss>` before overwriting, mirroring the PowerShell behavior documented in [installer/README.md](../../../installer/README.md) (being replaced in Task 14).

- [ ] **Step 1: Write the failing tests**

```csharp
using System.Text.Json.Nodes;
using SefimMcp.Setup;
using Xunit;

namespace SefimMcp.Tests;

public class ClientConfigWriterTests
{
    [Fact]
    public void UpdateJsonHost_PreservesForeignKeys_AndOtherServers()
    {
        var dir = Directory.CreateTempSubdirectory("sefim-client-config");
        try
        {
            var path = Path.Combine(dir.FullName, "claude_desktop_config.json");
            File.WriteAllText(path, """
                {
                  "theme": "dark",
                  "mcpServers": {
                    "other-server": { "command": "other.exe" }
                  }
                }
                """);

            ClientConfigWriter.UpdateJsonHost(path, "sefim", "/opt/sefim/sefim-ai-mcp", "full");

            var result = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            Assert.Equal("dark", (string)result["theme"]!);
            Assert.Equal("other.exe", (string)result["mcpServers"]!["other-server"]!["command"]!);
            Assert.Equal("/opt/sefim/sefim-ai-mcp", (string)result["mcpServers"]!["sefim"]!["command"]!);
            Assert.Equal("full", (string)result["mcpServers"]!["sefim"]!["env"]!["SEFIM_TOOL_PROFILE"]!);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void UpdateJsonHost_RemovesStaleVersionedEntry_MatchingSefimBinary()
    {
        var dir = Directory.CreateTempSubdirectory("sefim-client-config-stale");
        try
        {
            var path = Path.Combine(dir.FullName, "claude_desktop_config.json");
            File.WriteAllText(path, """
                {
                  "mcpServers": {
                    "sefim-old": { "command": "/opt/sefim-mcp/0.1.0-beta/sefim-ai-mcp" }
                  }
                }
                """);

            ClientConfigWriter.UpdateJsonHost(path, "sefim", "/opt/sefim-mcp/0.2.0/sefim-ai-mcp", "full");

            var result = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            Assert.False(result["mcpServers"]!.AsObject().ContainsKey("sefim-old"));
            Assert.True(result["mcpServers"]!.AsObject().ContainsKey("sefim"));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void RemoveJsonHost_DropsOnlySefimEntry()
    {
        var dir = Directory.CreateTempSubdirectory("sefim-client-config-remove");
        try
        {
            var path = Path.Combine(dir.FullName, "claude_desktop_config.json");
            File.WriteAllText(path, """
                {
                  "mcpServers": {
                    "sefim": { "command": "/opt/sefim/sefim-ai-mcp" },
                    "keep-me": { "command": "keep.exe" }
                  }
                }
                """);

            ClientConfigWriter.RemoveJsonHost(path, "sefim");

            var result = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            Assert.False(result["mcpServers"]!.AsObject().ContainsKey("sefim"));
            Assert.True(result["mcpServers"]!.AsObject().ContainsKey("keep-me"));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test tests/SefimMcp.Tests/SefimMcp.Tests.csproj --filter ClientConfigWriterTests`
Expected: FAIL — types do not exist.

- [ ] **Step 3: Implement `AppSettingsWriter`**

```csharp
using System.Text.Json.Nodes;

namespace SefimMcp.Setup;

public static class AppSettingsWriter
{
    public static void Update(string appSettingsPath, string connectionString, string? imageLocation)
    {
        var root = ReadJsonObject(appSettingsPath);

        var sql = GetOrCreateObject(root, "SqlService");
        sql["ConnectionString"] = connectionString;

        if (!string.IsNullOrWhiteSpace(imageLocation))
        {
            var sefim = GetOrCreateObject(root, "SEFIM");
            sefim["ImageLocation"] = imageLocation;
        }

        SaveJson(appSettingsPath, root);
    }

    internal static JsonObject ReadJsonObject(string path)
    {
        if (!File.Exists(path))
            return new JsonObject();

        var raw = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(raw))
            return new JsonObject();

        return JsonNode.Parse(raw) as JsonObject ?? new JsonObject();
    }

    internal static JsonObject GetOrCreateObject(JsonObject parent, string key)
    {
        if (parent[key] is JsonObject existing)
            return existing;

        var created = new JsonObject();
        parent[key] = created;
        return created;
    }

    internal static void SaveJson(string path, JsonObject root)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        if (File.Exists(path))
        {
            var backup = $"{path}.sefim-backup-{DateTime.Now:yyyyMMddHHmmss}";
            File.Copy(path, backup, overwrite: true);
        }

        var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(path, root.ToJsonString(options));
    }
}
```

- [ ] **Step 4: Implement `ClientConfigWriter`**

```csharp
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace SefimMcp.Setup;

public static class ClientConfigWriter
{
    public static string UpdateJsonHost(string configPath, string serverKey, string exePath, string toolProfile)
    {
        var config = AppSettingsWriter.ReadJsonObject(configPath);
        var servers = AppSettingsWriter.GetOrCreateObject(config, "mcpServers");

        // An older release lives in a different versioned folder; drop it so the host does not
        // keep launching a stale server binary next to the newly installed one.
        foreach (var key in servers.Select(kv => kv.Key).ToList())
        {
            if (key == serverKey) continue;
            var command = (string?)servers[key]?["command"];
            if (command is not null && Regex.IsMatch(command, "(?i)SEFIM-MCP|sefim-ai-mcp"))
                servers.Remove(key);
        }

        var entry = new JsonObject
        {
            ["command"] = exePath,
            ["env"] = new JsonObject { ["SEFIM_TOOL_PROFILE"] = toolProfile },
        };
        servers[serverKey] = entry;

        AppSettingsWriter.SaveJson(configPath, config);
        return configPath;
    }

    public static bool RemoveJsonHost(string configPath, string serverKey)
    {
        if (!File.Exists(configPath))
            return false;

        var config = AppSettingsWriter.ReadJsonObject(configPath);
        if (config["mcpServers"] is not JsonObject servers)
            return false;

        var changed = false;
        foreach (var key in servers.Select(kv => kv.Key).ToList())
        {
            var command = (string?)servers[key]?["command"];
            if (key == serverKey || (command is not null && Regex.IsMatch(command, "(?i)SEFIM-MCP|sefim-ai-mcp")))
            {
                servers.Remove(key);
                changed = true;
            }
        }

        if (changed)
            AppSettingsWriter.SaveJson(configPath, config);
        return changed;
    }

    public static void UpdateCodexToml(string configPath, string serverKey, string exePath, string toolProfile, bool remove)
    {
        if (!File.Exists(configPath))
            return;

        var text = File.ReadAllText(configPath);
        var backup = $"{configPath}.sefim-backup-{DateTime.Now:yyyyMMddHHmmss}";
        File.Copy(configPath, backup, overwrite: true);

        var pattern = $@"(?ms)^\[mcp_servers\.{Regex.Escape(serverKey)}\].*?(?=^\[|\z)";
        text = Regex.Replace(text, pattern, "");

        if (!remove)
        {
            var escapedExe = exePath.Replace("\\", "\\\\");
            var block = $"[mcp_servers.{serverKey}]\n" +
                        $"command = \"{escapedExe}\"\n" +
                        "args = []\n" +
                        $"env = {{ SEFIM_TOOL_PROFILE = \"{toolProfile}\" }}\n";
            text = text.TrimEnd() + "\n\n" + block;
        }

        File.WriteAllText(configPath, text.TrimStart());
    }
}
```

- [ ] **Step 5: Run tests, verify pass**

Run: `dotnet test tests/SefimMcp.Tests/SefimMcp.Tests.csproj --filter ClientConfigWriterTests`
Expected: PASS (3/3).

- [ ] **Step 6: Commit**

```bash
git add sefim-ai-mcp/Setup/AppSettingsWriter.cs sefim-ai-mcp/Setup/ClientConfigWriter.cs tests/SefimMcp.Tests/ClientConfigWriterTests.cs
git commit -m "feat(setup): AOT-safe JsonNode-based appsettings and MCP client config writers"
```

---

### Task 5: `SqlConnectionTester`

**Files:**
- Create: `sefim-ai-mcp/Setup/SqlConnectionTester.cs`

**Interfaces:**
- Consumes: `Microsoft.Data.SqlClient` (already a transitive dependency via the `SqlService` project reference — confirmed in `SERVICES/SqlService/SqlService.csproj:74`).
- Produces:
  ```csharp
  public sealed record SqlTestResult(bool Success, string Message);
  public static class SqlConnectionTester
  {
      public static async Task<SqlTestResult> TestAsync(string server, string database, string? userId, string? password, CancellationToken ct);
  }
  ```
  No unit test: this task requires a live SQL Server, which is not available in CI. It is exercised only through Task 6's `setup test` mode manually and through the QtIFW install-time log (Task 9). Do not fake a test that mocks `SqlConnection` — that is exactly the kind of test the project's own `Configure-SefimMcp.ps1` never had either; keep parity, not false coverage.

- [ ] **Step 1: Implement**

```csharp
using Microsoft.Data.SqlClient;

namespace SefimMcp.Setup;

public sealed record SqlTestResult(bool Success, string Message);

public static class SqlConnectionTester
{
    public static async Task<SqlTestResult> TestAsync(string server, string database, string? userId, string? password, CancellationToken ct)
    {
        var auth = string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(password)
            ? "Integrated Security=True;"
            : $"User Id={userId};Password={password};";
        var connectionString = $"Server={server};Database={database};{auth}" +
                                "TrustServerCertificate=True;Encrypt=True;Connection Timeout=8;Application Name=SefimMcpSetup;";

        await using var connection = new SqlConnection(connectionString);
        try
        {
            await connection.OpenAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT DB_NAME()";
            var name = (string?)await command.ExecuteScalarAsync(ct) ?? database;
            return new SqlTestResult(true, $"{server} üzerindeki {name} veritabanına bağlanıldı.");
        }
        catch (Exception ex)
        {
            return new SqlTestResult(false, ex.Message);
        }
    }
}
```

- [ ] **Step 2: Build check**

Run: `dotnet build sefim-ai-mcp/sefim-ai-mcp.csproj -c Release`
Expected: builds cleanly (no test — see rationale above).

- [ ] **Step 3: Commit**

```bash
git add sefim-ai-mcp/Setup/SqlConnectionTester.cs
git commit -m "feat(setup): add cross-platform SQL Server connectivity probe"
```

---

### Task 6: `SetupCommandRunner` — CLI dispatch, wired into `Program.cs`

**Files:**
- Create: `sefim-ai-mcp/Setup/SetupCommandRunner.cs`
- Modify: `sefim-ai-mcp/Program.cs:1-14`
- Test: `tests/SefimMcp.Tests/SetupCommandRunnerTests.cs`

**Interfaces:**
- Consumes: `SetupPaths`, `SefimDetection`, `AppSettingsWriter`, `ClientConfigWriter`, `SqlConnectionTester` (Tasks 2–5).
- Produces: `public static class SetupCommandRunner { public static Task<bool> TryRunAsync(string[] args, CancellationToken ct); }` — same shape as `KnowledgeCommandRunner.TryRunAsync`, called the same way from `Program.cs`.
- CLI shape (all named args, matching the flag style already used for `knowledge pack --output`):
  ```
  sefim-ai-mcp setup detect
  sefim-ai-mcp setup configure --install-dir <dir> --host claude|chatgpt --sefim-dir <dir> --pro-images <dir>
                                --server <s> --database <d> [--user-id <u>] [--password <p>]
                                [--tool-profile full|core] [--server-key sefim]
  sefim-ai-mcp setup test --server <s> --database <d> [--user-id <u>] [--password <p>]
  sefim-ai-mcp setup remove [--host claude|chatgpt] [--server-key sefim]
  ```
  Every mode writes `key=value` lines to stdout (one per line, no file-based result channel needed anymore — QtIFW captures process stdout directly via `installer.execute()`, unlike NSIS which needed the UTF-16LE result-file workaround for `nsExec`). Exit code `0` on success, `1` on failure with a `status=error` / `message=...` pair.

- [ ] **Step 1: Write the failing tests**

```csharp
using SefimMcp.Setup;
using Xunit;

namespace SefimMcp.Tests;

public class SetupCommandRunnerTests
{
    [Fact]
    public async Task TryRunAsync_ReturnsFalse_WhenFirstArgIsNotSetup()
    {
        var handled = await SetupCommandRunner.TryRunAsync(new[] { "knowledge", "stats" }, CancellationToken.None);
        Assert.False(handled);
    }

    [Fact]
    public async Task TryRunAsync_Configure_WritesAppSettingsAndHostConfig()
    {
        var dir = Directory.CreateTempSubdirectory("sefim-setup-configure");
        try
        {
            var exeName = OperatingSystem.IsWindows() ? "sefim-ai-mcp.exe" : "sefim-ai-mcp";
            File.WriteAllText(Path.Combine(dir.FullName, exeName), "stub");
            File.WriteAllText(Path.Combine(dir.FullName, "appsettings.json"), """{ "SqlService": {}, "SEFIM": {} }""");
            var hostConfig = Path.Combine(dir.FullName, "host.json");

            var args = new[]
            {
                "setup", "configure",
                "--install-dir", dir.FullName,
                "--host-config", hostConfig,
                "--server", "SQLHOST",
                "--database", "SefimDb",
                "--pro-images", dir.FullName,
                "--tool-profile", "full",
                "--server-key", "sefim",
            };

            var handled = await SetupCommandRunner.TryRunAsync(args, CancellationToken.None);

            Assert.True(handled);
            Assert.Contains("Server=SQLHOST", File.ReadAllText(Path.Combine(dir.FullName, "appsettings.json")));
            Assert.Contains("\"sefim\"", File.ReadAllText(hostConfig));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task TryRunAsync_Configure_MissingServer_FailsWithNonZeroExitCode()
    {
        var args = new[] { "setup", "configure", "--install-dir", Path.GetTempPath(), "--host-config", Path.GetTempFileName() };

        await SetupCommandRunner.TryRunAsync(args, CancellationToken.None);

        Assert.NotEqual(0, Environment.ExitCode);
        Environment.ExitCode = 0; // reset for subsequent tests in the same process
    }
}
```

> Note: `--host-config` is a direct-path test seam so the unit test does not depend on real per-user profile
> directories; the QtIFW `--host claude|chatgpt` path (Task 9's real CLI surface) resolves the actual path via
> `SetupPaths.ClaudeConfigPath()`/`ChatGptConfigPath()` when `--host-config` is not given.

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test tests/SefimMcp.Tests/SefimMcp.Tests.csproj --filter SetupCommandRunnerTests`
Expected: FAIL — `SetupCommandRunner` does not exist.

- [ ] **Step 3: Implement**

```csharp
namespace SefimMcp.Setup;

public static class SetupCommandRunner
{
    public static async Task<bool> TryRunAsync(string[] args, CancellationToken ct)
    {
        if (args.Length < 2 || !args[0].Equals("setup", StringComparison.OrdinalIgnoreCase))
            return false;

        switch (args[1].ToLowerInvariant())
        {
            case "detect":
                RunDetect();
                return true;
            case "test":
                await RunTestAsync(args, ct);
                return true;
            case "configure":
                RunConfigure(args);
                return true;
            case "remove":
                RunRemove(args);
                return true;
            default:
                Console.Error.WriteLine("Usage: setup detect | setup test | setup configure | setup remove");
                Environment.ExitCode = 2;
                return true;
        }
    }

    private static void RunDetect()
    {
        var sefimDir = SefimDetection.FindSefimDirectory();
        var proImages = SefimDetection.FindProImages(sefimDir);
        var connection = SefimDetection.ReadConnectionString(sefimDir);

        WritePair("sefimdir", sefimDir ?? "");
        WritePair("proimages", proImages ?? "");
        WritePair("server", connection.Server ?? "");
        WritePair("database", connection.Database ?? "");
        WritePair("userid", connection.UserId ?? "");
        WritePair("password", connection.Password ?? "");
        WritePair("claudeconfig", SetupPaths.ClaudeConfigPath() ?? "");
        WritePair("chatgptconfig", SetupPaths.ChatGptConfigPath() ?? "");
        WritePair("status", "ok");
    }

    private static async Task RunTestAsync(string[] args, CancellationToken ct)
    {
        var server = GetOption(args, "--server");
        var database = GetOption(args, "--database");
        var userId = GetOption(args, "--user-id");
        var password = GetOption(args, "--password");

        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database))
        {
            Fail("Server ve database alanları zorunludur.");
            return;
        }

        var result = await SqlConnectionTester.TestAsync(server, database, userId, password, ct);
        WritePair("status", result.Success ? "ok" : "error");
        WritePair("message", result.Message);
        if (!result.Success)
            Environment.ExitCode = 1;
    }

    private static void RunConfigure(string[] args)
    {
        var installDir = GetOption(args, "--install-dir");
        if (string.IsNullOrWhiteSpace(installDir))
        {
            Fail("--install-dir gereklidir.");
            return;
        }

        var server = GetOption(args, "--server");
        var database = GetOption(args, "--database");
        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database))
        {
            Fail("--server ve --database gereklidir.");
            return;
        }

        var exeName = OperatingSystem.IsWindows() ? "sefim-ai-mcp.exe" : "sefim-ai-mcp";
        var exePath = Path.Combine(installDir, exeName);
        if (!File.Exists(exePath))
        {
            Fail($"Sunucu çalıştırılabilir dosyası bulunamadı: {exePath}");
            return;
        }

        var userId = GetOption(args, "--user-id");
        var password = GetOption(args, "--password");
        var proImages = GetOption(args, "--pro-images");
        var toolProfile = GetOption(args, "--tool-profile") ?? "full";
        var serverKey = GetOption(args, "--server-key") ?? "sefim";
        var host = GetOption(args, "--host");

        var connectionString = SefimDetection.BuildConnectionString(server, database, userId, password);
        AppSettingsWriter.Update(Path.Combine(installDir, "appsettings.json"), connectionString, proImages);
        WritePair("appsettings", Path.Combine(installDir, "appsettings.json"));

        var hostConfigPath = GetOption(args, "--host-config") ?? host switch
        {
            "claude" => SetupPaths.ClaudeConfigPath(),
            "chatgpt" => SetupPaths.ChatGptConfigPath(),
            _ => null,
        };

        if (!string.IsNullOrWhiteSpace(hostConfigPath))
        {
            ClientConfigWriter.UpdateJsonHost(hostConfigPath, serverKey, exePath, toolProfile);
            WritePair("hostconfig", hostConfigPath);

            if (host == "chatgpt")
            {
                var codexPath = SetupPaths.CodexConfigPath();
                if (File.Exists(codexPath))
                    ClientConfigWriter.UpdateCodexToml(codexPath, serverKey, exePath, toolProfile, remove: false);
            }
        }

        WritePair("status", "ok");
    }

    private static void RunRemove(string[] args)
    {
        var serverKey = GetOption(args, "--server-key") ?? "sefim";
        var host = GetOption(args, "--host");

        foreach (var (name, path) in new[]
                 {
                     ("claude", SetupPaths.ClaudeConfigPath()),
                     ("chatgpt", SetupPaths.ChatGptConfigPath()),
                 })
        {
            if (host is not null && host != name) continue;
            if (path is not null && ClientConfigWriter.RemoveJsonHost(path, serverKey))
                WritePair("cleaned", path);
        }

        var codexPath = SetupPaths.CodexConfigPath();
        if (File.Exists(codexPath))
            ClientConfigWriter.UpdateCodexToml(codexPath, serverKey, exePath: "", toolProfile: "", remove: true);

        WritePair("status", "ok");
    }

    private static void Fail(string message)
    {
        WritePair("status", "error");
        WritePair("message", message);
        Environment.ExitCode = 1;
    }

    private static void WritePair(string key, string value)
        => Console.WriteLine($"{key}={value.Replace("\r", " ").Replace("\n", " ")}");

    private static string? GetOption(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
```

- [ ] **Step 4: Wire into `Program.cs`**

```csharp
// sefim-ai-mcp/Program.cs, immediately after the existing knowledge-command check:
if (await KnowledgeCommandRunner.TryRunAsync(args, builder.Configuration, CancellationToken.None))
    return;

if (await SetupCommandRunner.TryRunAsync(args, CancellationToken.None))
    return;
```

Add `<Using Include="SefimMcp.Setup" />` next to the other `<Using>` entries in `sefim-ai-mcp/sefim-ai-mcp.csproj` (same `ItemGroup` as `SefimMcp.Knowledge.Authoring`).

- [ ] **Step 5: Run tests, verify pass**

Run: `dotnet test tests/SefimMcp.Tests/SefimMcp.Tests.csproj --filter SetupCommandRunnerTests`
Expected: PASS (3/3).

- [ ] **Step 6: Full test suite still green**

Run: `dotnet test tests/SefimMcp.Tests/SefimMcp.Tests.csproj`
Expected: all tests pass (existing + new).

- [ ] **Step 7: Commit**

```bash
git add sefim-ai-mcp/Setup/SetupCommandRunner.cs sefim-ai-mcp/Program.cs sefim-ai-mcp/sefim-ai-mcp.csproj tests/SefimMcp.Tests/SetupCommandRunnerTests.cs
git commit -m "feat(setup): add cross-platform 'setup' CLI subcommand replacing Configure-SefimMcp.ps1"
```

---

### Task 7: Delete the NSIS installer tree and its workflow

**Files:**
- Delete: `installer/sefim-mcp.nsi`
- Delete: `installer/build-installer.sh` (old NSIS-calling version — Task 11 creates a new one at `installer/qtifw/build-installer.sh`)
- Delete: `installer/scripts/Configure-SefimMcp.ps1`
- Delete: `installer/tests/Test-Configure.ps1`
- Delete: `installer/assets/generate-assets.py` (old ICO/BMP version — Task 10 creates a QtIFW-appropriate replacement)
- Delete: `installer/assets/header.bmp`, `installer/assets/welcome.bmp`, `installer/assets/install.ico`, `installer/assets/uninstall.ico`
- Delete: `.github/workflows/windows-installer.yml` (Task 13 creates the replacement)

This task removes dead weight before new files land in new paths, so `git log` for `installer/` stays legible instead of showing simultaneous delete+create noise inside one commit.

- [ ] **Step 1: Remove the files**

```bash
git rm -r installer/sefim-mcp.nsi installer/build-installer.sh installer/scripts installer/tests installer/assets .github/workflows/windows-installer.yml
```

- [ ] **Step 2: Confirm nothing else references them**

Run: `grep -rn "sefim-mcp.nsi\|Configure-SefimMcp.ps1\|windows-installer.yml" --include=*.md --include=*.yml --include=*.cs .`
Expected: no matches outside this plan file itself.

- [ ] **Step 3: Commit**

```bash
git commit -m "chore: remove Windows-only NSIS installer, replaced by cross-platform QtIFW setup"
```

---

### Task 8: QtIFW package metadata (`config.xml`, `package.xml`)

**Files:**
- Create: `installer/qtifw/config/config.xml`
- Create: `installer/qtifw/packages/com.ozfiliz.sefimmcp/meta/package.xml`

**Interfaces:**
- Consumes: `<Name>` in `package.xml` (`com.ozfiliz.sefimmcp`) is referenced by `installscript.qs` (Task 9) and by `build-installer.sh` (Task 11) when it copies the published payload into `packages/com.ozfiliz.sefimmcp/data/`.

- [ ] **Step 1: Write `config.xml`**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Installer>
    <Name>Şefim MCP Server</Name>
    <Version>0.1.0-beta</Version>
    <Title>Şefim MCP Server Kurulumu</Title>
    <Publisher>OZFİLİZ YAZILIM</Publisher>
    <ProductUrl>https://github.com/yasinakmaz/sefim-mcp-ai</ProductUrl>
    <InstallerApplicationIcon>../assets/installer</InstallerApplicationIcon>
    <InstallerWindowIcon>../assets/installer.png</InstallerWindowIcon>
    <Logo>../assets/logo.png</Logo>
    <!-- Per-user install: no admin/root elevation requested on any platform. -->
    <TargetDir>@ApplicationsDirUser@/SefimMcp/@ProductVersion@</TargetDir>
    <AllowNonAsciiCharacters>true</AllowNonAsciiCharacters>
    <AllowSpaceInPath>true</AllowSpaceInPath>
    <RunProgram>@TargetDir@/sefim-ai-mcp</RunProgram>
    <RunProgramArguments>
        <Argument>--version</Argument>
    </RunProgramArguments>
    <RunProgramDescription>Kurulumu doğrula</RunProgramDescription>
    <StartMenuDir>Şefim MCP Server</StartMenuDir>
    <MaintenanceToolName>SefimMcpUninstall</MaintenanceToolName>
    <WizardStyle>Modern</WizardStyle>
</Installer>
```

> `@ApplicationsDirUser@` resolves to the per-user applications directory on every OS (confirmed predefined
> variable — see Global Constraints). `RunProgram`/`RunProgramArguments` intentionally point at a harmless
> `--version` check rather than launching the stdio MCP server, which would hang waiting on stdin if the
> Finish-page "Run now" checkbox is left ticked.

- [ ] **Step 2: Write `package.xml`**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Package>
    <DisplayName>Şefim MCP Server</DisplayName>
    <Description>Şefim POS verilerini yapay zekâ istemcilerine bağlayan MCP sunucusu.</Description>
    <Version>0.1.0-beta</Version>
    <ReleaseDate>2026-09-01</ReleaseDate>
    <Name>com.ozfiliz.sefimmcp</Name>
    <Default>true</Default>
    <ForcedInstallation>true</ForcedInstallation>
    <Script>installscript.qs</Script>
    <UserInterfaces>
        <UserInterface>ClientPage.ui</UserInterface>
        <UserInterface>SefimPage.ui</UserInterface>
    </UserInterfaces>
</Package>
```

> `<Version>`/`<ReleaseDate>` are placeholders overwritten by `build-installer.sh` (Task 11) with `sed` before
> `binarycreator` runs, the same way the old `sefim-mcp.nsi` took `-DVERSION` on the command line.

- [ ] **Step 3: Commit**

```bash
git add installer/qtifw/config/config.xml installer/qtifw/packages/com.ozfiliz.sefimmcp/meta/package.xml
git commit -m "feat(installer): add QtIFW config.xml and package.xml"
```

---

### Task 9: QtIFW wizard pages — `ClientPage.ui`, `SefimPage.ui`, `installscript.qs`

**Files:**
- Create: `installer/qtifw/packages/com.ozfiliz.sefimmcp/meta/ClientPage.ui`
- Create: `installer/qtifw/packages/com.ozfiliz.sefimmcp/meta/SefimPage.ui`
- Create: `installer/qtifw/packages/com.ozfiliz.sefimmcp/meta/installscript.qs`

**Interfaces:**
- Consumes: `SetupCommandRunner` CLI surface from Task 6 (`setup configure --install-dir ... --host ... --sefim-dir ... --pro-images ... --server ... --database ... --user-id ... --password ... --tool-profile ... --server-key sefim`), invoked as `@TargetDir@/sefim-ai-mcp` (`.exe` appended on Windows by the script, see below).
- Produces: two custom pages inserted before `QInstaller.ReadyForInstallation`, and a `Component.prototype.createOperations` that appends the install-time `Execute` call.

- [ ] **Step 1: Write `ClientPage.ui`** (radio-button host selection, mirrors the NSIS `HostPageCreate` page)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<ui version="4.0">
 <class>ClientPage</class>
 <widget class="QWidget" name="ClientPage">
  <layout class="QVBoxLayout">
   <item>
    <widget class="QLabel" name="introLabel">
     <property name="text">
      <string>Sunucu, seçtiğiniz uygulamanın yapılandırma dosyasına eklenir. Dosyadaki diğer sunucular ve ayarlar korunur.</string>
     </property>
     <property name="wordWrap"><bool>true</bool></property>
    </widget>
   </item>
   <item>
    <widget class="QRadioButton" name="claudeRadio">
     <property name="text"><string>Claude Desktop kullan</string></property>
     <property name="checked"><bool>true</bool></property>
    </widget>
   </item>
   <item>
    <widget class="QRadioButton" name="chatgptRadio">
     <property name="text"><string>ChatGPT Desktop kullan</string></property>
    </widget>
   </item>
   <item>
    <widget class="QLabel" name="noteLabel">
     <property name="text">
      <string>Not: Değişikliğin etkili olması için kurulum bittikten sonra seçtiğiniz uygulamayı tamamen kapatıp yeniden açın.</string>
     </property>
     <property name="wordWrap"><bool>true</bool></property>
    </widget>
   </item>
  </layout>
 </widget>
</ui>
```

- [ ] **Step 2: Write `SefimPage.ui`** (Sefim dir, images dir, server/database/user/password — user/password optional, mirrors and extends the NSIS `SefimPageCreate` page with a dedicated images field)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<ui version="4.0">
 <class>SefimPage</class>
 <widget class="QWidget" name="SefimPage">
  <layout class="QGridLayout">
   <item row="0" column="0" colspan="3">
    <widget class="QLabel" name="sefimDirLabel"><property name="text"><string>Şefim kurulum klasörü (varsa)</string></property></widget>
   </item>
   <item row="1" column="0" colspan="2"><widget class="QLineEdit" name="sefimDirEdit"/></item>
   <item row="1" column="2"><widget class="QPushButton" name="sefimDirBrowse"><property name="text"><string>Gözat...</string></property></widget></item>

   <item row="2" column="0" colspan="3">
    <widget class="QLabel" name="imagesLabel"><property name="text"><string>Görseller (proimages) klasörü</string></property></widget>
   </item>
   <item row="3" column="0" colspan="2"><widget class="QLineEdit" name="imagesEdit"/></item>
   <item row="3" column="2"><widget class="QPushButton" name="imagesBrowse"><property name="text"><string>Gözat...</string></property></widget></item>

   <item row="4" column="0"><widget class="QLabel" name="serverLabel"><property name="text"><string>Sunucu *</string></property></widget></item>
   <item row="4" column="1" colspan="2"><widget class="QLineEdit" name="serverEdit"/></item>

   <item row="5" column="0"><widget class="QLabel" name="databaseLabel"><property name="text"><string>Veritabanı *</string></property></widget></item>
   <item row="5" column="1" colspan="2"><widget class="QLineEdit" name="databaseEdit"/></item>

   <item row="6" column="0"><widget class="QLabel" name="userLabel"><property name="text"><string>Kullanıcı (opsiyonel)</string></property></widget></item>
   <item row="6" column="1" colspan="2"><widget class="QLineEdit" name="userEdit"/></item>

   <item row="7" column="0"><widget class="QLabel" name="passwordLabel"><property name="text"><string>Parola (opsiyonel)</string></property></widget></item>
   <item row="7" column="1" colspan="2"><widget class="QLineEdit" name="passwordEdit"><property name="echoMode"><enum>QLineEdit::Password</enum></property></widget></item>

   <item row="8" column="0" colspan="3">
    <widget class="QLabel" name="hintLabel">
     <property name="text"><string>Kullanıcı ve parola boş bırakılırsa Integrated Security kullanılır. Sunucu ve veritabanı zorunludur.</string></property>
     <property name="wordWrap"><bool>true</bool></property>
    </widget>
   </item>
  </layout>
 </widget>
</ui>
```

- [ ] **Step 3: Write `installscript.qs`**

```javascript
function Component()
{
    installer.addWizardPage(component, "ClientPage", QInstaller.TargetDirectory);
    installer.addWizardPage(component, "SefimPage", QInstaller.TargetDirectory);
}

// Pre-fills OS-specific default guesses without touching the filesystem — the real
// scan (Detect) runs at install time via the C# 'setup detect' operation below, so a
// stale guess here is never authoritative, only a starting point for the user to edit.
function defaultSefimDir()
{
    if (systemInfo.kernelType === "winnt")
        return "C:/Program Files (x86)/Vega/Sefim";
    return "";
}

function defaultImagesDir()
{
    if (systemInfo.kernelType === "winnt")
        return "C:/Program Files (x86)/Vega/Sefim/proimages";
    if (systemInfo.kernelType === "darwin")
        return "/Volumes/Sefim/proimages";
    return "/mnt/proimages";
}

Component.prototype.ClientPageCallback = function()
{
    var page = gui.pageWidgetByObjectName("DynamicClientPage");
    if (page == null)
        return;
    page.claudeRadio.checked = true;
}

Component.prototype.SefimPageCallback = function()
{
    var page = gui.pageWidgetByObjectName("DynamicSefimPage");
    if (page == null)
        return;
    if (page.sefimDirEdit.text.length === 0)
        page.sefimDirEdit.text = defaultSefimDir();
    if (page.imagesEdit.text.length === 0)
        page.imagesEdit.text = defaultImagesDir();

    page.sefimDirBrowse.clicked.connect(function() {
        var dir = QFileDialog.getExistingDirectory("Şefim kurulum klasörünü seçin", page.sefimDirEdit.text);
        if (dir.length > 0)
            page.sefimDirEdit.text = dir;
    });
    page.imagesBrowse.clicked.connect(function() {
        var dir = QFileDialog.getExistingDirectory("Görseller klasörünü seçin", page.imagesEdit.text);
        if (dir.length > 0)
            page.imagesEdit.text = dir;
    });

    // Server + Database stay mandatory; User/Password are optional (Integrated Security fallback,
    // see SefimMcp.Setup.SefimDetection.BuildConnectionString).
    page.serverEdit.textChanged.connect(function() { validateSefimPage(page); });
    page.databaseEdit.textChanged.connect(function() { validateSefimPage(page); });
    validateSefimPage(page);
}

function validateSefimPage(page)
{
    var ok = page.serverEdit.text.length > 0 && page.databaseEdit.text.length > 0;
    gui.currentPageWidget().complete = ok;
}

Component.prototype.createOperations = function()
{
    component.createOperations();

    var exeName = (systemInfo.kernelType === "winnt") ? "sefim-ai-mcp.exe" : "sefim-ai-mcp";
    var clientPage = gui.pageWidgetByObjectName("DynamicClientPage");
    var sefimPage = gui.pageWidgetByObjectName("DynamicSefimPage");

    var host = (clientPage != null && clientPage.chatgptRadio.checked) ? "chatgpt" : "claude";
    var sefimDir = (sefimPage != null) ? sefimPage.sefimDirEdit.text : "";
    var proImages = (sefimPage != null) ? sefimPage.imagesEdit.text : "";
    var server = (sefimPage != null) ? sefimPage.serverEdit.text : "";
    var database = (sefimPage != null) ? sefimPage.databaseEdit.text : "";
    var userId = (sefimPage != null) ? sefimPage.userEdit.text : "";
    var password = (sefimPage != null) ? sefimPage.passwordEdit.text : "";

    var args = [
        "@TargetDir@/" + exeName, "setup", "configure",
        "--install-dir", "@TargetDir@",
        "--host", host,
        "--sefim-dir", sefimDir,
        "--pro-images", proImages,
        "--server", server,
        "--database", database,
        "--tool-profile", "full",
        "--server-key", "sefim"
    ];
    if (userId.length > 0) { args.push("--user-id"); args.push(userId); }
    if (password.length > 0) { args.push("--password"); args.push(password); }

    // Runs after the implicit Extract operation added by component.createOperations() above,
    // so @TargetDir@/<exeName> already exists on disk. Non-fatal by design: a configuration
    // failure must not roll back a successful file install, matching the old NSIS behavior
    // (Section only showed a MessageBox warning, it never aborted).
    component.addOperation("Execute", args.concat(["{0,1}"]));
}

function Controller() {}

Controller.prototype.FinishedPageCallback = function()
{
    gui.clickButton(buttons.FinishButton);
}
```

> The commented-out `addElevatedOperation` idea from earlier drafts was dropped: per-user installs must never
> trigger a UAC/pkexec prompt, so this uses plain `component.addOperation("Execute", ...)` with `"{0,1}"` telling
> QtIFW that exit codes 0 or 1 both count as success (1 = "configured but SQL test/detection had a soft warning",
> matching the C# `Fail()` helper's `Environment.ExitCode = 1`, never a hard installer failure).

- [ ] **Step 4: Commit**

```bash
git add installer/qtifw/packages/com.ozfiliz.sefimmcp/meta/ClientPage.ui installer/qtifw/packages/com.ozfiliz.sefimmcp/meta/SefimPage.ui installer/qtifw/packages/com.ozfiliz.sefimmcp/meta/installscript.qs
git commit -m "feat(installer): add QtIFW custom wizard pages and install-time setup invocation"
```

---

### Task 10: Installer assets (icons/logo/watermark)

**Files:**
- Create: `installer/qtifw/assets/generate-assets.py`
- Generate (by running the script, not by hand): `installer/qtifw/assets/logo.png`, `installer/qtifw/assets/installer.png`, `installer/qtifw/assets/installer.icns`, `installer/qtifw/assets/installer.ico`

**Interfaces:** none — pure asset generation, consumed by `config.xml` (Task 8) paths `../assets/installer`, `../assets/installer.png`, `../assets/logo.png`.

- [ ] **Step 1: Port the old `installer/assets/generate-assets.py`** (deleted in Task 7) to emit PNG/ICO/ICNS instead of BMP/ICO. Read the deleted file's git history for the exact drawing logic before writing the replacement:

Run: `git log --diff-filter=D -- installer/assets/generate-assets.py` to find the deletion commit, then `git show <commit>~1:installer/assets/generate-assets.py` to recover the old content, and reuse its color palette / text so the new installer keeps the same visual identity, just re-exported for QtIFW's expected formats (`Pillow`'s `Image.save(..., format="ICNS")` for macOS, `format="ICO"` with multiple sizes for Windows, plain `.png` for the Linux/generic logo and window icon).

- [ ] **Step 2: Run it and verify outputs exist**

Run: `python3 installer/qtifw/assets/generate-assets.py`
Expected: `installer/qtifw/assets/{logo.png,installer.png,installer.ico,installer.icns}` all exist and are non-empty.

- [ ] **Step 3: Commit**

```bash
git add installer/qtifw/assets/generate-assets.py installer/qtifw/assets/logo.png installer/qtifw/assets/installer.png installer/qtifw/assets/installer.ico installer/qtifw/assets/installer.icns
git commit -m "feat(installer): regenerate wizard assets for QtIFW (PNG/ICO/ICNS)"
```

---

### Task 11: Cross-platform `build-installer.sh`

**Files:**
- Create: `installer/qtifw/build-installer.sh`

**Interfaces:**
- Consumes: a published payload directory (e.g. `artifacts/linux-x64/`) containing `sefim-ai-mcp[.exe]`, `appsettings.json`, optional `knowledge.pack`; and a version string for stamping `package.xml`/`config.xml`.
- Produces: an installer binary named `SefimMcpSetup-<version>-<label>.<ext>` in the repo root.

- [ ] **Step 1: Write the script**

```bash
#!/usr/bin/env bash
# Builds a QtIFW installer from an already-published, single-RID payload.
#
# Usage:
#   installer/qtifw/build-installer.sh <payload-dir> <version> <os-arch-label>
#
# Example:
#   installer/qtifw/build-installer.sh artifacts/linux-x64 0.1.0-beta linux-x64
#
# os-arch-label controls only the output filename; the actual target platform is
# whatever OS this script is invoked on (binarycreator is not cross-platform).

set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
qtifw_dir="$repo_root/installer/qtifw"
payload_dir="$1"
version="$2"
label="$3"

if [[ ! -f "$payload_dir/sefim-ai-mcp" && ! -f "$payload_dir/sefim-ai-mcp.exe" ]]; then
    echo "Payload not found under $payload_dir (expected sefim-ai-mcp or sefim-ai-mcp.exe)" >&2
    exit 1
fi

# --- 1. Ensure binarycreator is available -----------------------------------
tools_dir="$repo_root/.qtifw-tools"
if ! command -v binarycreator >/dev/null 2>&1 && [[ ! -x "$tools_dir/bin/binarycreator" && ! -x "$tools_dir/bin/binarycreator.exe" ]]; then
    echo "Installing Qt Installer Framework via aqtinstall..."
    python3 -m pip install --quiet --upgrade aqtinstall
    case "$(uname -s)" in
        Linux*)  host=linux ;;
        Darwin*) host=mac ;;
        *)       host=windows ;;
    esac
    python3 -m aqt install-tool "$host" desktop tools_ifw --outputdir "$tools_dir"
fi

bc="binarycreator"
if ! command -v binarycreator >/dev/null 2>&1; then
    bc="$(find "$tools_dir" -iname 'binarycreator*' -type f | head -1)"
fi

# --- 2. Stamp version into package.xml / config.xml --------------------------
work_dir="$(mktemp -d)"
trap 'rm -rf "$work_dir"' EXIT
cp -r "$qtifw_dir/config" "$qtifw_dir/packages" "$work_dir/"

sed -i.bak "s/<Version>.*<\/Version>/<Version>${version}<\/Version>/" \
    "$work_dir/config/config.xml" "$work_dir/packages/com.ozfiliz.sefimmcp/meta/package.xml"
rm -f "$work_dir/config/config.xml.bak" "$work_dir/packages/com.ozfiliz.sefimmcp/meta/package.xml.bak"

# --- 3. Stage payload into the package's data/ dir ----------------------------
data_dir="$work_dir/packages/com.ozfiliz.sefimmcp/data"
mkdir -p "$data_dir"
cp -r "$payload_dir"/. "$data_dir/"

# Knowledge is only ever shipped as the encrypted knowledge.pack; refuse to package
# plaintext markdown, mirroring the check the old windows-installer.yml workflow ran.
if find "$data_dir" -iname '*.md' | grep -q .; then
    echo "Refusing to package plaintext knowledge (*.md found under payload)." >&2
    find "$data_dir" -iname '*.md' >&2
    exit 1
fi

# --- 4. Build ------------------------------------------------------------------
case "$(uname -s)" in
    Linux*)  ext="run" ;;
    Darwin*) ext="app" ;;
    *)       ext="exe" ;;
esac
output="$repo_root/SefimMcpSetup-${version}-${label}.${ext}"

"$bc" --offline-only -c "$work_dir/config/config.xml" -p "$work_dir/packages" "$output"

echo "Setup written to: $output"
```

- [ ] **Step 2: Make executable**

```bash
chmod +x installer/qtifw/build-installer.sh
```

Do not run it in this task — no published payload exists yet (that requires `dotnet publish`, exercised in Task 15's manual verification).

- [ ] **Step 3: Commit**

```bash
git add installer/qtifw/build-installer.sh
git commit -m "feat(installer): add cross-platform QtIFW build script (aqtinstall-bootstrapped)"
```

---

### Task 12: `.gitignore` entries for build artifacts

**Files:**
- Modify: `.gitignore`

- [ ] **Step 1: Add entries**

```gitignore
# QtIFW installer build
/.qtifw-tools/
/artifacts/
/SefimMcpSetup-*
installer/qtifw/packages/com.ozfiliz.sefimmcp/data/
```

- [ ] **Step 2: Commit**

```bash
git add .gitignore
git commit -m "chore: ignore QtIFW build artifacts and staged payload data"
```

---

### Task 13: GitHub Actions — `release.yml` production matrix

**Files:**
- Create: `.github/workflows/release.yml`

**Interfaces:**
- Consumes: `installer/qtifw/build-installer.sh <payload-dir> <version> <label>` (Task 11), `sefim-ai-mcp/sefim-ai-mcp.csproj` RIDs (Task 1), the existing `knowledge.pack` encryption/header contract (`SEFIMKP1`, unchanged from `windows-installer.yml`).

- [ ] **Step 1: Write the workflow**

```yaml
name: Release installers

on:
  push:
    tags:
      - 'v*'
  workflow_dispatch:
    inputs:
      version:
        description: 'Setup version (defaults to PackageVersion in the csproj)'
        required: false
        type: string

permissions:
  contents: write

jobs:
  version:
    runs-on: ubuntu-latest
    outputs:
      version: ${{ steps.resolve.outputs.version }}
      allow-missing-pack: ${{ steps.pack.outputs.allow-missing }}
    steps:
      - uses: actions/checkout@v4
      - name: Resolve version
        id: resolve
        run: |
          version="${{ inputs.version }}"
          if [[ -z "$version" && "${GITHUB_REF}" == refs/tags/v* ]]; then
            version="${GITHUB_REF#refs/tags/v}"
          fi
          if [[ -z "$version" ]]; then
            version="$(sed -n 's:.*<PackageVersion>\(.*\)</PackageVersion>.*:\1:p' sefim-ai-mcp/sefim-ai-mcp.csproj | head -1)"
          fi
          if [[ -z "$version" ]]; then echo "Version could not be resolved." >&2; exit 1; fi
          echo "version=$version" >> "$GITHUB_OUTPUT"
      - name: Check knowledge pack
        id: pack
        run: |
          pack=sefim-ai-mcp/knowledge.pack
          if [[ -f "$pack" ]]; then
            header="$(head -c 8 "$pack")"
            if [[ "$header" != "SEFIMKP1" ]]; then echo "knowledge.pack missing SEFIMKP1 header" >&2; exit 1; fi
            echo "allow-missing=false" >> "$GITHUB_OUTPUT"
          else
            echo "allow-missing=true" >> "$GITHUB_OUTPUT"
            echo "::warning::knowledge.pack is missing; setups will ship without business knowledge."
          fi

  build:
    needs: version
    strategy:
      fail-fast: false
      matrix:
        include:
          - os: windows-latest
            rid: win-x64
            label: win-x64
          - os: windows-latest
            rid: win-arm64
            label: win-arm64
          - os: ubuntu-latest
            rid: linux-x64
            label: linux-x64
          - os: ubuntu-24.04-arm
            rid: linux-arm64
            label: linux-arm64
          - os: macos-13
            rid: osx-x64
            label: macos-x64
          - os: macos-14
            rid: osx-arm64
            label: macos-arm64
    runs-on: ${{ matrix.os }}
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore sefim-ai-mcp.sln

      - name: Test
        if: matrix.rid == 'linux-x64'
        run: dotnet test tests/SefimMcp.Tests/SefimMcp.Tests.csproj -c Release --no-restore

      - name: Publish ${{ matrix.rid }}
        shell: bash
        run: |
          dotnet publish sefim-ai-mcp/sefim-ai-mcp.csproj \
            -c Release -r ${{ matrix.rid }} --self-contained true \
            -p:AllowMissingKnowledgePack=${{ needs.version.outputs.allow-missing-pack }} \
            -p:PackageVersion=${{ needs.version.outputs.version }} \
            -o artifacts/${{ matrix.rid }}

      - name: Verify payload
        shell: bash
        run: |
          exe="artifacts/${{ matrix.rid }}/sefim-ai-mcp"
          [[ "${{ matrix.rid }}" == win-* ]] && exe="${exe}.exe"
          [[ -f "$exe" ]] || { echo "Publish output is missing $exe" >&2; exit 1; }
          [[ -f "artifacts/${{ matrix.rid }}/appsettings.json" ]] || { echo "appsettings.json missing" >&2; exit 1; }
          if find "artifacts/${{ matrix.rid }}" -iname '*.md' | grep -q .; then
            echo "Plaintext knowledge in payload:" >&2
            find "artifacts/${{ matrix.rid }}" -iname '*.md' >&2
            exit 1
          fi
          if [[ "${{ needs.version.outputs.allow-missing-pack }}" == "false" ]]; then
            packed="artifacts/${{ matrix.rid }}/knowledge.pack"
            [[ -f "$packed" ]] || { echo "knowledge.pack did not reach publish output" >&2; exit 1; }
            header="$(head -c 8 "$packed")"
            [[ "$header" == "SEFIMKP1" ]] || { echo "Published knowledge.pack is not encrypted" >&2; exit 1; }
          fi

      - name: Install Qt Installer Framework
        shell: bash
        run: |
          python3 -m pip install --quiet --upgrade aqtinstall
          case "${{ matrix.os }}" in
            windows-*) host=windows ;;
            macos-*)   host=mac ;;
            *)         host=linux ;;
          esac
          python3 -m aqt install-tool "$host" desktop tools_ifw --outputdir .qtifw-tools

      - name: Build installer
        shell: bash
        run: |
          chmod +x installer/qtifw/build-installer.sh
          installer/qtifw/build-installer.sh "artifacts/${{ matrix.rid }}" "${{ needs.version.outputs.version }}" "${{ matrix.label }}"

      - name: Upload installer artifact
        uses: actions/upload-artifact@v4
        with:
          name: SefimMcpSetup-${{ needs.version.outputs.version }}-${{ matrix.label }}
          path: SefimMcpSetup-${{ needs.version.outputs.version }}-${{ matrix.label }}.*
          if-no-files-found: error

  release:
    needs: [version, build]
    if: startsWith(github.ref, 'refs/tags/v')
    runs-on: ubuntu-latest
    steps:
      - uses: actions/download-artifact@v4
        with:
          path: dist
          merge-multiple: true
      - name: Attach to release
        uses: softprops/action-gh-release@v2
        with:
          files: dist/*
```

- [ ] **Step 2: Validate YAML syntax**

Run: `python3 -c "import yaml,sys; yaml.safe_load(open('.github/workflows/release.yml'))"`
Expected: no exception.

- [ ] **Step 3: Commit**

```bash
git add .github/workflows/release.yml
git commit -m "ci: replace Windows-only NSIS workflow with cross-platform QtIFW release matrix"
```

---

### Task 14: `installer/README.md` rewrite

**Files:**
- Modify (content fully replaced): `installer/README.md`

- [ ] **Step 1: Write the new README** covering: per-user install locations per OS, the `setup` CLI subcommand surface (Task 6), how `installscript.qs` invokes it (Task 9), local build instructions (`dotnet publish ... && installer/qtifw/build-installer.sh ...`), and the release workflow (Task 13). Base the structure on the pre-Task-7 file (`git show <task-7-commit>~1:installer/README.md`) but replace every NSIS/PowerShell-specific section.

- [ ] **Step 2: Commit**

```bash
git add installer/README.md
git commit -m "docs: rewrite installer README for the QtIFW setup"
```

---

### Task 15: End-to-end manual verification (one pass per OS family)

This task has no unit tests — it is the actual acceptance check for the whole plan, matching how the old NSIS installer could only be verified by running it.

- [ ] **Step 1: Local Linux build + run**

```bash
dotnet publish sefim-ai-mcp/sefim-ai-mcp.csproj -c Release -r linux-x64 --self-contained true -p:AllowMissingKnowledgePack=true -o artifacts/linux-x64
installer/qtifw/build-installer.sh artifacts/linux-x64 0.0.0-dev linux-x64
./SefimMcpSetup-0.0.0-dev-linux-x64.run
```
Expected: wizard opens, Client page and Sefim/DB page render, Next is blocked until Server+Database are filled, install completes into the resolved `@ApplicationsDirUser@/SefimMcp/0.0.0-dev` path (record the actual observed path here), `appsettings.json` inside it has the entered connection string, and `~/.config/Claude/claude_desktop_config.json` (or wherever `SetupPaths.ClaudeConfigPath()` actually resolves at runtime) gained a `mcpServers.sefim` entry.

- [ ] **Step 2: Record any QtIFW API corrections needed**

If `gui.currentPageWidget().complete = ok` inside `validateSefimPage` did not correctly gate the Next button, re-verify the exact completion-flag mechanism against Context7 (`/websites/doc_qt_io`, query "QtIFW dynamic page complete property Next button validation") before changing `installscript.qs`.

- [ ] **Step 3: Repeat on Windows and macOS**

Same steps with `win-x64`/`osx-arm64` payloads on native machines (native AOT cannot be tested via emulation). Record install path, config path, and any OS-specific `installscript.qs` corrections back into Task 9's file.

- [ ] **Step 4: Commit any corrections found during verification**

```bash
git add installer/qtifw/packages/com.ozfiliz.sefimmcp/meta/installscript.qs
git commit -m "fix(installer): correct QtIFW API usage found during manual cross-platform verification"
```

---

## Self-Review Notes

- **Spec coverage:** NSIS full parity → Tasks 6, 9 (client selection, Sefim detection, connectionstring parsing, appsettings/client config writing, uninstall/remove via `setup remove`). Images path selectable + OS-specific defaults → Task 2 (`ProImagesCandidates`), Task 9 (`imagesEdit` field + `defaultImagesDir()`). User/password optional → Task 3 (`BuildConnectionString`), Task 5, Task 6, Task 9 (`hintLabel`). Delete everything NSIS-related → Task 7. Professional cross-platform CI, Apple Silicon+Intel, Linux x64+arm64, Windows x64+arm64, release attachment, encrypted-pack-only, no markdown leak → Task 13.
- **Placeholder scan:** the one place plain English replaces code is Task 15, where QtIFW's exact page-completion-flag mechanism cannot be confirmed without actually running the built installer — this is marked as an explicit manual-verification step with a named Context7 query to run if it's wrong, not a silent TODO.
- **Type consistency:** `SetupCommandRunner` CLI flags (`--install-dir`, `--host`, `--sefim-dir`, `--pro-images`, `--server`, `--database`, `--user-id`, `--password`, `--tool-profile`, `--server-key`) are used identically in Task 6's implementation and Task 9's `installscript.qs` argument array.

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-09-01-cross-platform-qtifw-installer.md`. Two execution options:

1. **Subagent-Driven (recommended)** — a fresh subagent per task, review between tasks, fast iteration.
2. **Inline Execution** — execute tasks in this session using executing-plans, batch execution with checkpoints.

Which approach?
