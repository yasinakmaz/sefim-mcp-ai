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
