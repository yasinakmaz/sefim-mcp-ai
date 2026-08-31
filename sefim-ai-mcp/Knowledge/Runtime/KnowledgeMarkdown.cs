namespace SefimMcp.Knowledge.Runtime;

/// <summary>Small helpers for reading structure out of an authored knowledge document.</summary>
public static class KnowledgeMarkdown
{
    /// <summary>Returns the body of a top-level <c>## heading</c> section, or null when the section is absent or empty.</summary>
    public static string? ExtractSection(string body, string heading)
    {
        var lines = body.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var index = Array.FindIndex(lines, line => line.Trim().Equals($"## {heading}", StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return null;

        var section = string.Join('\n', lines.Skip(index + 1).TakeWhile(line => !line.StartsWith("## ", StringComparison.Ordinal))).Trim();
        return section.Length == 0 ? null : section;
    }
}
