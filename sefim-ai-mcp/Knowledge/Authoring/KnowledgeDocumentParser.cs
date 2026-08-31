using SefimMcp.Knowledge.Models;
using SefimMcp.Knowledge.Runtime;

namespace SefimMcp.Knowledge.Authoring;

public static class KnowledgeDocumentParser
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
    private static readonly string[] RequiredFields = ["kind", "id", "status", "exposure"];
    private static readonly string[] KnownKinds = ["application", "glossary", "guidance", "table", "workflow", "business-rule"];

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

        foreach (var required in RequiredFields)
        {
            if (!metadata.TryGetValue(required, out var value) || string.IsNullOrWhiteSpace(value))
            {
                issue = new(sourceName, "missing_field", $"Frontmatter field '{required}' is required.");
                return false;
            }
        }

        if (!KnownKinds.Contains(metadata["kind"], Comparer))
        {
            issue = new(sourceName, "unknown_kind", $"Kind '{metadata["kind"]}' is not one of: {string.Join(", ", KnownKinds)}.");
            return false;
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

        var summary = metadata.TryGetValue("summary", out var summaryValue) && !IsAuthoringPlaceholder(summaryValue)
            ? summaryValue
            : null;
        var aliases = metadata.TryGetValue("aliases", out var aliasValue)
            ? aliasValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : [];

        document = new(metadata["kind"], metadata["id"], metadata["status"], metadata["exposure"], title, body, metadata, sourceName, summary, aliases);

        return true;
    }

    public static KnowledgeValidationResult ValidateDirectory(string directory)
    {
        var issues = new List<KnowledgeValidationIssue>();
        if (!Directory.Exists(directory))
            return new(false, [new(directory, "missing_directory", "Knowledge source directory does not exist.")]);

        var seenIds = new Dictionary<string, string>(Comparer);
        foreach (var file in Directory.EnumerateFiles(directory, "*.md", SearchOption.AllDirectories))
        {
            var sourceName = Path.GetRelativePath(directory, file);
            if (!TryParse(sourceName, File.ReadAllText(file), out var document, out var issue))
            {
                if (issue is not null)
                    issues.Add(issue);
                continue;
            }

            var key = $"{document!.Kind}:{document.Id}";
            if (seenIds.TryGetValue(key, out var firstSource))
            {
                issues.Add(new(sourceName, "duplicate_id", $"Identifier '{document.Id}' of kind '{document.Kind}' is already defined in {firstSource}."));
                continue;
            }

            seenIds[key] = sourceName;
        }

        return new(issues.Count == 0, issues);
    }

    /// <summary>Authoring templates keep their prompts in HTML comments; those are never treated as content.</summary>
    private static bool IsAuthoringPlaceholder(string value) =>
        value.Contains("<!--", StringComparison.Ordinal) || value.Contains("USER:", StringComparison.Ordinal);
}
