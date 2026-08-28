using SefimMcp.Knowledge.Models;

namespace SefimMcp.Knowledge.Authoring;

public static class KnowledgeDocumentParser
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    public static bool TryParse(string sourceName, string markdown, out KnowledgeDocument? document, out KnowledgeValidationIssue? issue)
    {
        document = null;
        issue = null;
        var normalized = markdown.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            issue = new(sourceName, "missing_frontmatter", "Document must begin with YAML frontmatter.");
            return false;
        }

        var frontmatterEnd = normalized.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (frontmatterEnd < 0)
        {
            issue = new(sourceName, "invalid_frontmatter", "Frontmatter closing delimiter is missing.");
            return false;
        }

        var metadata = new Dictionary<string, string>(Comparer);
        foreach (var line in normalized[4..frontmatterEnd].Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = line.IndexOf(':');
            if (separator <= 0)
            {
                issue = new(sourceName, "invalid_frontmatter", "Frontmatter supports only simple key: value fields.");
                return false;
            }

            metadata[line[..separator].Trim()] = line[(separator + 1)..].Trim().Trim('"');
        }

        foreach (var required in new[] { "kind", "id", "status", "exposure" })
        {
            if (!metadata.TryGetValue(required, out var value) || string.IsNullOrWhiteSpace(value))
            {
                issue = new(sourceName, "missing_field", $"Frontmatter field '{required}' is required.");
                return false;
            }
        }

        if (!string.Equals(metadata["exposure"], "model", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(metadata["exposure"], "private", StringComparison.OrdinalIgnoreCase))
        {
            issue = new(sourceName, "invalid_exposure", "Exposure must be model or private.");
            return false;
        }

        var body = normalized[(frontmatterEnd + 5)..].Trim();
        var title = body.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(static line => line.StartsWith("# ", StringComparison.Ordinal))?[2..].Trim()
            ?? metadata["id"];
        document = new(metadata["kind"], metadata["id"], metadata["status"], metadata["exposure"], title, body, metadata, sourceName);
        return true;
    }

    public static KnowledgeValidationResult ValidateDirectory(string directory)
    {
        var issues = new List<KnowledgeValidationIssue>();
        if (!Directory.Exists(directory))
            return new(false, [new(directory, "missing_directory", "Knowledge source directory does not exist.")]);

        foreach (var file in Directory.EnumerateFiles(directory, "*.md", SearchOption.AllDirectories))
        {
            if (!TryParse(Path.GetRelativePath(directory, file), File.ReadAllText(file), out _, out var issue) && issue is not null)
                issues.Add(issue);
        }

        return new(issues.Count == 0, issues);
    }
}
