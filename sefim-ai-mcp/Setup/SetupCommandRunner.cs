namespace SefimMcp.Setup;

public static class SetupCommandRunner
{
    public static async Task<bool> TryRunAsync(string[] args, CancellationToken ct)
    {
        if (args.Length < 2 || !args[0].Equals("setup", StringComparison.OrdinalIgnoreCase))
            return false;

        var mode = args[1].ToLowerInvariant();
        if (mode is not ("detect" or "test" or "configure" or "remove"))
        {
            Console.Error.WriteLine("Usage: setup detect | setup test | setup configure | setup remove");
            Environment.ExitCode = 2;
            return true;
        }

        // Every known mode must exit 0 or 1: the installer's Execute operation only tolerates
        // "{0,1}", and a signal-level crash there reopens the infinite Retry/Ignore/Cancel loop.
        try
        {
            switch (mode)
            {
                case "detect":
                    RunDetect();
                    break;
                case "test":
                    await RunTestAsync(args, ct);
                    break;
                case "configure":
                    await RunConfigureAsync(args);
                    break;
                case "remove":
                    RunRemove(args);
                    break;
            }
        }
        catch (Exception ex)
        {
            Fail(ex.Message);
        }

        return true;
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

    private static async Task RunConfigureAsync(string[] args)
    {
        var installDir = GetOption(args, "--install-dir");
        if (string.IsNullOrWhiteSpace(installDir))
        {
            Fail("--install-dir gereklidir.");
            return;
        }

        var server = GetOption(args, "--server");
        var database = GetOption(args, "--database");
        var userId = GetOption(args, "--user-id");
        var password = GetOption(args, "--password");
        var sefimDir = GetOption(args, "--sefim-dir");

        // The wizard collects the connection fields as plain text (no pre-install process
        // execution is possible), so 'setup detect' never runs before this point. Fall back to
        // reading Şefim's own connectionstring.txt for whatever the caller left blank.
        if ((string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database))
            && !string.IsNullOrWhiteSpace(sefimDir))
        {
            var detected = SefimDetection.ReadConnectionString(sefimDir);
            if (string.IsNullOrWhiteSpace(server)) server = detected.Server;
            if (string.IsNullOrWhiteSpace(database)) database = detected.Database;
            if (string.IsNullOrWhiteSpace(userId)) userId = detected.UserId;
            if (string.IsNullOrWhiteSpace(password)) password = detected.Password;
        }

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

        var proImages = GetOption(args, "--pro-images");
        if (string.IsNullOrWhiteSpace(proImages))
            proImages = SefimDetection.FindProImages(sefimDir);
        var toolProfile = GetOption(args, "--tool-profile") ?? "full";
        var serverKey = GetOption(args, "--server-key") ?? "sefim";
        var host = GetOption(args, "--host");

        var connectionString = SefimDetection.BuildConnectionString(server, database, userId, password);

        // Informational only: the install log should show whether the DB is reachable, but a
        // failed probe (VPN down, SQL service not started yet, malformed server name) must never
        // fail the install — hence the local catch, which also keeps it out of the top-level
        // handler in TryRunAsync that would otherwise turn it into status=error.
        try
        {
            var sqlTest = await SqlConnectionTester.TestAsync(server, database, userId, password, CancellationToken.None);
            WritePair("sqltest", sqlTest.Success ? "ok" : $"warning: {sqlTest.Message}");
        }
        catch (Exception ex)
        {
            WritePair("sqltest", $"warning: {ex.Message}");
        }

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
