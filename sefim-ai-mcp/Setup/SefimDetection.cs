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
