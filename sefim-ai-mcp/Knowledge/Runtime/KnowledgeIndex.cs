using SefimMcp.Knowledge.Models;

namespace SefimMcp.Knowledge.Runtime;

/// <summary>
/// In-memory BM25 index over knowledge sections.
/// BM25 replaces raw substring counting because it discounts terms that appear in every document
/// ("ürün", "şefim") and rewards rare business terms, which is what discovery queries actually key on.
/// The whole corpus is a few hundred kilobytes, so the index is built once per process at first use.
/// </summary>
public sealed class KnowledgeIndex
{
    private const double K1 = 1.2;
    private const double B = 0.75;
    private const double HeadingWeight = 3.0;
    private const double MetadataSectionWeight = 2.5;
    private const double PrefixMatchDamping = 0.7;

    private readonly List<Entry> entries;
    private readonly Dictionary<string, List<Posting>> postings;
    private readonly Dictionary<string, KnowledgeDocument> documentsById;
    private readonly double averageLength;

    private KnowledgeIndex(
        List<Entry> entries,
        Dictionary<string, List<Posting>> postings,
        Dictionary<string, KnowledgeDocument> documentsById,
        double averageLength)
    {
        this.entries = entries;
        this.postings = postings;
        this.documentsById = documentsById;
        this.averageLength = averageLength;
    }

    public static KnowledgeIndex Build(IReadOnlyList<KnowledgeDocument> documents)
    {
        var entries = new List<Entry>();
        var documentsById = new Dictionary<string, KnowledgeDocument>(StringComparer.OrdinalIgnoreCase);

        foreach (var document in documents)
        {
            documentsById[document.Id] = document;

            // A synthetic entry carrying identifiers, title, aliases and summary. It lets a query hit the document
            // through the words the author expects ("adisyon", "cari") even when the body phrases things differently.
            var metadataText = string.Join(' ', new[]
            {
                document.Id,
                document.Id.Replace('.', ' '),
                document.Title,
                document.Kind,
                document.Summary ?? string.Empty,
                document.Aliases is null ? string.Empty : string.Join(' ', document.Aliases)
            });
            entries.Add(Entry.Create(document, "Identity", metadataText, MetadataSectionWeight));

            foreach (var section in KnowledgeChunker.Split(document))
                entries.Add(Entry.Create(document, section.Heading, section.Text, 1.0));
        }

        var postings = new Dictionary<string, List<Posting>>(StringComparer.Ordinal);
        for (var index = 0; index < entries.Count; index++)
        {
            foreach (var (term, frequency) in entries[index].Frequencies)
            {
                if (!postings.TryGetValue(term, out var list))
                    postings[term] = list = [];

                list.Add(new(index, frequency));
            }
        }

        var averageLength = entries.Count == 0 ? 1.0 : entries.Average(entry => (double)entry.Length);
        return new(entries, postings, documentsById, averageLength <= 0 ? 1.0 : averageLength);
    }

    public IReadOnlyList<KnowledgeSearchHit> Search(string query, int limit, int maximumSectionsPerDocument = 2)
    {
        var queryTerms = KnowledgeText.Tokenize(query).Distinct(StringComparer.Ordinal).ToArray();
        if (queryTerms.Length == 0 || entries.Count == 0)
            return [];

        var scores = new Dictionary<int, double>();
        foreach (var queryTerm in queryTerms)
        {
            foreach (var (term, list) in postings)
            {
                if (!KnowledgeText.IsPrefixMatch(term, queryTerm))
                    continue;

                var damping = term.Equals(queryTerm, StringComparison.Ordinal) ? 1.0 : PrefixMatchDamping;
                var idf = Math.Log(1 + (entries.Count - list.Count + 0.5) / (list.Count + 0.5));
                foreach (var posting in list)
                {
                    var entry = entries[posting.EntryIndex];
                    var normalized = posting.Frequency * (K1 + 1) /
                                     (posting.Frequency + K1 * (1 - B + B * entry.Length / averageLength));
                    scores[posting.EntryIndex] = scores.GetValueOrDefault(posting.EntryIndex) +
                                                 idf * normalized * entry.Weight * damping;
                }
            }
        }

        var perDocument = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var hits = new List<KnowledgeSearchHit>();
        foreach (var (entryIndex, score) in scores.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key))
        {
            if (hits.Count >= limit)
                break;

            var entry = entries[entryIndex];
            var used = perDocument.GetValueOrDefault(entry.DocumentId);
            if (used >= maximumSectionsPerDocument)
                continue;

            perDocument[entry.DocumentId] = used + 1;
            var document = documentsById[entry.DocumentId];
            hits.Add(new(
                document.Kind,
                document.Id,
                document.Title,
                document.Status,
                Snippet(entry.Heading == "Identity" ? document.Summary ?? entry.Text : entry.Text),
                (int)Math.Round(score * 100),
                entry.Heading,
                BuildUri(document, entry.Heading)));
        }

        return hits;
    }

    private static string BuildUri(KnowledgeDocument document, string heading) =>
        $"sefim://knowledge/{Uri.EscapeDataString(document.Kind)}/{Uri.EscapeDataString(document.Id)}#{Uri.EscapeDataString(heading)}";

    private static string Snippet(string text)
    {
        var collapsed = text.Replace("\n", " ", StringComparison.Ordinal).Trim();
        return collapsed.Length <= 400 ? collapsed : string.Concat(collapsed.AsSpan(0, 397), "...");
    }

    private readonly record struct Posting(int EntryIndex, int Frequency);

    private sealed record Entry(
        string DocumentId,
        string Heading,
        string Text,
        double Weight,
        Dictionary<string, int> Frequencies,
        int Length)
    {
        public static Entry Create(KnowledgeDocument document, string heading, string text, double weight)
        {
            var tokens = KnowledgeText.Tokenize(text);

            // The heading is repeated so that "Write Guidance" or a glossary term outranks a passing mention
            // of the same words inside a long body paragraph.
            var headingTokens = KnowledgeText.Tokenize(heading);
            var frequencies = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var token in tokens)
                frequencies[token] = frequencies.GetValueOrDefault(token) + 1;

            foreach (var token in headingTokens)
                frequencies[token] = frequencies.GetValueOrDefault(token) + (int)HeadingWeight;

            return new(document.Id, heading, text, weight, frequencies, Math.Max(1, tokens.Count));
        }
    }
}
