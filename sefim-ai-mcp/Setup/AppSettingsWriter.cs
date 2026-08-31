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
