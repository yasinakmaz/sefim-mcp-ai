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

        if (host is null || host == "chatgpt")
        {
            var codexPath = SetupPaths.CodexConfigPath();
            if (File.Exists(codexPath))
                ClientConfigWriter.UpdateCodexToml(codexPath, serverKey, exePath: "", toolProfile: "", remove: true);
        }

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
