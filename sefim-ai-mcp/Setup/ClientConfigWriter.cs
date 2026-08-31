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
