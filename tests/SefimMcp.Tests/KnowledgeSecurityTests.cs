using System.Security.Cryptography;
using ModelContextProtocol.Server;
using SefimMcp.Knowledge.Authoring;
using SefimMcp.Knowledge.Models;
using SefimMcp.Knowledge.Runtime;
using SefimMcp.Knowledge.Security;
using Xunit;

namespace SefimMcp.Tests;

public sealed class KnowledgeSecurityTests
{
    [Fact]
    public void Parser_requires_expected_frontmatter()
    {
        var success = KnowledgeDocumentParser.TryParse("bad.md", "# Missing", out _, out var issue);
        Assert.False(success);
        Assert.Equal("missing_frontmatter", issue?.Code);
    }

    [Fact]
    public void Parser_recognizes_draft_table()
    {
        const string markdown = "---\nkind: table\nid: dbo.Product\nstatus: draft\nexposure: model\n---\n# Product\n\n## Purpose\nDraft";
        var success = KnowledgeDocumentParser.TryParse("dbo.Product.md", markdown, out var document, out var issue);
        Assert.True(success, issue?.Message);
        Assert.Equal("draft", document?.Status);
        Assert.Equal("dbo.Product", document?.Id);
    }

    [Fact]
    public void Encrypted_pack_rejects_tampering()
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var pack = new KnowledgePack([new("application", "sefim", "draft", "model", "Sefim", "content", new Dictionary<string, string>(), "overview.md")], DateTimeOffset.UtcNow);
        var encrypted = KnowledgePackCodec.Encrypt(pack, key);
        encrypted[^1] ^= 0x01;
        Assert.ThrowsAny<CryptographicException>(() => KnowledgePackCodec.Decrypt(encrypted, key));
    }

    [Fact]
    public void Source_controlled_configuration_contains_no_password()
    {
        var root = FindRepositoryRoot();
        var configuration = File.ReadAllText(Path.Combine(root, "sefim-ai-mcp", "appsettings.json"));
        Assert.DoesNotContain("Password=", configuration, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Embedded_key_opens_the_shipped_pack()
    {
        // The pack in source control has to match the key compiled into the server:
        // repacking after a key change is easy to forget and only shows up at runtime.
        var packPath = Path.Combine(FindRepositoryRoot(), "sefim-ai-mcp", "knowledge.pack");
        Assert.True(File.Exists(packPath), $"knowledge.pack is missing: {packPath}");

        Assert.True(new EmbeddedKnowledgeKeyProvider(environmentVariable: null).TryGetKey(out var key));
        Assert.Equal(32, key.Length);

        var pack = KnowledgePackCodec.Decrypt(File.ReadAllBytes(packPath), key);
        Assert.NotEmpty(pack.Documents);
    }

    [Fact]
    public void Mcp_adapters_expose_inherited_legacy_tool_contracts()
    {
        var hasTool = typeof(SefimMcp.Mcp.Tools.ReportMcpTools).GetMethods()
            .Any(method => method.GetCustomAttributes(typeof(McpServerToolAttribute), inherit: true).Length > 0);
        Assert.True(hasTool);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "sefim-ai-mcp.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
