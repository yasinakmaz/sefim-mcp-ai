using System.Text.RegularExpressions;
using Xunit;

namespace SefimMcp.Tests;

/// <summary>
/// The packaging rules are a security boundary: templates ship as plaintext inside the NuGet package,
/// so any real business content written into them would ship unencrypted.
/// </summary>
public sealed class KnowledgePackagingTests
{
    private static readonly Regex SkeletonLine = new(
        @"^\s*(#{1,6}\s.*|---|(kind|id|status|exposure|title|summary|aliases|max_rows|version):.*|\|.*\||([-*]\s+)?(\*\*[^*]+\*\*:?)?\s*(<!--.*-->)?)\s*$",
        RegexOptions.Compiled);

    [Fact]
    public void Templates_contain_no_authored_content()
    {
        var templates = Directory.GetFiles(Path.Combine(RepositoryRoot(), "knowledge", "templates"), "*.md", SearchOption.AllDirectories);
        Assert.NotEmpty(templates);

        foreach (var template in templates)
        {
            foreach (var line in File.ReadAllLines(template))
                Assert.True(SkeletonLine.IsMatch(line), $"{Path.GetFileName(template)} contains authored content that would ship unencrypted: {line}");
        }
    }

    [Fact]
    public void Private_knowledge_is_git_ignored()
    {
        var gitignore = File.ReadAllText(Path.Combine(RepositoryRoot(), ".gitignore"));
        Assert.Contains("knowledge/private", gitignore, StringComparison.Ordinal);
        Assert.Contains("knowledge.pack", gitignore, StringComparison.Ordinal);
    }

    [Fact]
    public void Project_never_packs_private_knowledge()
    {
        var project = File.ReadAllText(Path.Combine(RepositoryRoot(), "sefim-ai-mcp", "sefim-ai-mcp.csproj"));
        var packedPrivate = Regex.Matches(project, @"<None Include=""[^""]*knowledge[\\/]+private[^""]*""[^>]*Pack=""true""");
        Assert.Empty(packedPrivate);
        Assert.Contains("VerifyKnowledgeTemplatesAreBlank", project, StringComparison.Ordinal);
        Assert.Contains("VerifyKnowledgePackPresent", project, StringComparison.Ordinal);
    }

    private static string RepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "sefim-ai-mcp.sln")))
                return directory.FullName;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
