using SefimMcp.Knowledge.Models;
using SefimMcp.Knowledge.Security;

namespace SefimMcp.Knowledge.Runtime;

public sealed class KnowledgeService(KnowledgePackOptions options, IKnowledgeKeyProvider keyProvider) : IKnowledgeService
{
    private readonly SemaphoreSlim loadGate = new(1, 1);
    private KnowledgePack? pack;

    public async Task<KnowledgeDocument?> GetApplicationOverviewAsync(CancellationToken cancellationToken) =>
        (await GetDocumentsAsync(cancellationToken)).FirstOrDefault(d => d.Kind.Equals("application", StringComparison.OrdinalIgnoreCase));

    public async Task<IReadOnlyList<KnowledgeSearchHit>> SearchAsync(string query, int limit, CancellationToken cancellationToken)
    {
        var tokens = Tokenize(query);
        if (tokens.Count == 0)
            return [];

        return (await GetDocumentsAsync(cancellationToken))
            .Select(document => new { Document = document, Score = Score(document, tokens) })
            .Where(candidate => candidate.Score > 0)
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Document.Id, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Clamp(limit, 1, 20))
            .Select(candidate => new KnowledgeSearchHit(candidate.Document.Kind, candidate.Document.Id, candidate.Document.Title, candidate.Document.Status, Summarize(candidate.Document.Body), candidate.Score))
            .ToArray();
    }

    public async Task<IReadOnlyList<KnowledgeDocument>> ListTablesAsync(CancellationToken cancellationToken) =>
        (await GetDocumentsAsync(cancellationToken)).Where(d => d.Kind.Equals("table", StringComparison.OrdinalIgnoreCase)).OrderBy(d => d.Id, StringComparer.OrdinalIgnoreCase).ToArray();

    public async Task<KnowledgeDocument?> GetTableAsync(string tableName, CancellationToken cancellationToken) =>
        (await GetDocumentsAsync(cancellationToken)).FirstOrDefault(d => d.Kind.Equals("table", StringComparison.OrdinalIgnoreCase) && d.Id.Equals(tableName, StringComparison.OrdinalIgnoreCase));

    public async Task<IReadOnlyList<KnowledgeDocument>> GetByKindAsync(string kind, string? id, CancellationToken cancellationToken) =>
        (await GetDocumentsAsync(cancellationToken)).Where(d => d.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase) && (id is null || d.Id.Equals(id, StringComparison.OrdinalIgnoreCase))).ToArray();

    private async Task<IReadOnlyList<KnowledgeDocument>> GetDocumentsAsync(CancellationToken cancellationToken)
    {
        if (pack is not null)
            return pack.Documents.Where(d => d.Exposure.Equals("model", StringComparison.OrdinalIgnoreCase)).ToArray();

        await loadGate.WaitAsync(cancellationToken);
        try
        {
            if (pack is null && File.Exists(options.PackPath) && keyProvider.TryGetKey(out var key))
            {
                try { pack = KnowledgePackCodec.Decrypt(await File.ReadAllBytesAsync(options.PackPath, cancellationToken), key); }
                finally { System.Security.Cryptography.CryptographicOperations.ZeroMemory(key); }
            }
            pack ??= new([], DateTimeOffset.MinValue);
            return pack.Documents.Where(d => d.Exposure.Equals("model", StringComparison.OrdinalIgnoreCase)).ToArray();
        }
        finally { loadGate.Release(); }
    }

    private static int Score(KnowledgeDocument document, IReadOnlyList<string> tokens)
    {
        var fields = new[] { (document.Id, 12), (document.Title, 8), (document.Body, 2) };
        return tokens.Sum(token => fields.Where(field => field.Item1.Contains(token, StringComparison.OrdinalIgnoreCase)).Sum(field => field.Item2));
    }

    private static IReadOnlyList<string> Tokenize(string text) => text.Split([' ', '\t', '\r', '\n', '-', '_', '.', ',', ';', ':', '/', '\\'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Where(t => t.Length > 1).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    private static string Summarize(string body) => body.Length <= 360 ? body : string.Concat(body.AsSpan(0, 357), "...");
}
