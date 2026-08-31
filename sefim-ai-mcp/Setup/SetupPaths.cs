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
