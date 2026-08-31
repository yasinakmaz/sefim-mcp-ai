using System.Text;
using SefimMcp.Knowledge.Models;

namespace SefimMcp.Knowledge.Runtime;

/// <summary>
/// Splits a Markdown document into heading-scoped sections.
/// Retrieval returns sections rather than whole documents so a model can pull "dbo.Product / Write Guidance"
/// without loading every column description with it.
/// </summary>
public static class KnowledgeChunker
{
    private const int MaximumSectionLength = 2000;

    public static IReadOnlyList<KnowledgeSection> Split(KnowledgeDocument document)
    {
        var sections = new List<KnowledgeSection>();
        var lines = document.Body.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var heading = "Overview";
        var buffer = new StringBuilder();

        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("## ", StringComparison.Ordinal))
            {
                Emit(sections, document, heading, buffer);
                heading = trimmed[3..].Trim();
                continue;
            }

            if (trimmed.StartsWith("### ", StringComparison.Ordinal))
            {
                Emit(sections, document, heading, buffer);
                heading = trimmed[4..].Trim();
                continue;
            }

            if (trimmed.StartsWith("# ", StringComparison.Ordinal))
                continue;

            buffer.AppendLine(line);
        }

        Emit(sections, document, heading, buffer);
        return sections;
    }

    private static void Emit(List<KnowledgeSection> sections, KnowledgeDocument document, string heading, StringBuilder buffer)
    {
        var text = buffer.ToString().Trim();
        buffer.Clear();
        if (text.Length == 0)
            return;

        // A glossary section is a list of independent definitions; keeping them together would make every term
        // match every other term. One definition per section keeps term lookup precise.
        if (document.Kind.Equals("glossary", StringComparison.OrdinalIgnoreCase))
        {
            var emittedTerm = false;
            foreach (var line in text.Split('\n'))
            {
                var term = TryReadDefinitionTerm(line);
                if (term is null)
                    continue;

                sections.Add(new(document.Id, document.Kind, document.Title, term, line.Trim().TrimStart('-', ' ')));
                emittedTerm = true;
            }

            if (emittedTerm)
                return;
        }

        foreach (var part in SplitLongText(text))
            sections.Add(new(document.Id, document.Kind, document.Title, heading, part));
    }

    private static string? TryReadDefinitionTerm(string line)
    {
        var trimmed = line.TrimStart();
        if (!trimmed.StartsWith("- **", StringComparison.Ordinal))
            return null;

        var end = trimmed.IndexOf("**", 4, StringComparison.Ordinal);
        if (end < 0)
            return null;

        var term = trimmed[4..end].TrimEnd(':', ' ');
        return term.Length == 0 ? null : term;
    }

    /// <summary>Keeps a single section small enough that returning it stays cheap, splitting on blank lines.</summary>
    private static IEnumerable<string> SplitLongText(string text)
    {
        if (text.Length <= MaximumSectionLength)
        {
            yield return text;
            yield break;
        }

        var builder = new StringBuilder();
        foreach (var paragraph in text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
        {
            if (builder.Length > 0 && builder.Length + paragraph.Length > MaximumSectionLength)
            {
                yield return builder.ToString().Trim();
                builder.Clear();
            }

            builder.AppendLine(paragraph).AppendLine();
        }

        if (builder.Length > 0)
            yield return builder.ToString().Trim();
    }
}
