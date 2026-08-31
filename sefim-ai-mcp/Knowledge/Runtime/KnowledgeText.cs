using System.Text;

namespace SefimMcp.Knowledge.Runtime;

/// <summary>
/// Text handling for Turkish knowledge documents.
/// Ordinal comparison is not enough here: "Ürün", "URUN" and "urun" have to collapse to one term, and the
/// dotted/dotless i pair makes <c>ToLowerInvariant</c> alone wrong for Turkish input.
/// </summary>
public static class KnowledgeText
{
    private const int MinimumTokenLength = 2;

    /// <summary>Folds Turkish letters to their ASCII base and lowercases, so query and document meet on one form.</summary>
    public static string Fold(ReadOnlySpan<char> value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(character switch
            {
                'ç' or 'Ç' => 'c',
                'ğ' or 'Ğ' => 'g',
                'ı' or 'I' => 'i',
                'i' or 'İ' => 'i',
                'ö' or 'Ö' => 'o',
                'ş' or 'Ş' => 's',
                'ü' or 'Ü' => 'u',
                'â' or 'Â' => 'a',
                'î' or 'Î' => 'i',
                'û' or 'Û' => 'u',
                _ => char.ToLowerInvariant(character)
            });
        }

        return builder.ToString();
    }

    /// <summary>Splits folded text into search terms, dropping markdown punctuation and one-character noise.</summary>
    public static List<string> Tokenize(string text)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        foreach (var character in Fold(text))
        {
            if (char.IsLetterOrDigit(character))
            {
                current.Append(character);
                continue;
            }

            Flush(tokens, current);
        }

        Flush(tokens, current);
        return tokens;
    }

    /// <summary>
    /// Turkish is agglutinative: "ürünün", "ürünler" and "ürün" are the same concept for retrieval purposes.
    /// A shared prefix of at least <paramref name="minimumPrefix"/> characters is a cheap, dependency-free stand-in
    /// for a stemmer and keeps false positives low on short function words.
    /// </summary>
    public static bool IsPrefixMatch(string indexedTerm, string queryTerm, int minimumPrefix = 4)
    {
        if (indexedTerm.Equals(queryTerm, StringComparison.Ordinal))
            return true;

        var shorter = Math.Min(indexedTerm.Length, queryTerm.Length);
        if (shorter < minimumPrefix)
            return false;

        return indexedTerm.AsSpan(0, shorter).SequenceEqual(queryTerm.AsSpan(0, shorter));
    }

    private static void Flush(List<string> tokens, StringBuilder current)
    {
        if (current.Length >= MinimumTokenLength)
            tokens.Add(current.ToString());

        current.Clear();
    }
}
