using System.Security.Cryptography;
using SefimMcp.Knowledge.Models;
using SefimMcp.Knowledge.Security;

namespace SefimMcp.Knowledge.Runtime;

public sealed class KnowledgeService(KnowledgePackOptions options, IKnowledgeKeyProvider keyProvider, ILogger<KnowledgeService> logger)
    : IKnowledgeService
{
    private readonly SemaphoreSlim loadGate = new(1, 1);
    private LoadedKnowledge? loaded;

    public async Task<KnowledgeDocument?> GetApplicationOverviewAsync(CancellationToken cancellationToken) =>
        (await LoadAsync(cancellationToken)).Documents
            .FirstOrDefault(document => document.Kind.Equals("application", StringComparison.OrdinalIgnoreCase));

    public async Task<IReadOnlyList<KnowledgeSearchHit>> SearchAsync(string query, int limit, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        return (await LoadAsync(cancellationToken)).Index.Search(query, Math.Clamp(limit, 1, 20));
    }

    public async Task<IReadOnlyList<KnowledgeDocument>> ListTablesAsync(CancellationToken cancellationToken) =>
        (await LoadAsync(cancellationToken)).Documents
            .Where(document => document.Kind.Equals("table", StringComparison.OrdinalIgnoreCase))
            .OrderBy(document => document.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public async Task<KnowledgeDocument?> GetTableAsync(string tableName, CancellationToken cancellationToken) =>
        (await LoadAsync(cancellationToken)).Documents
            .FirstOrDefault(document => document.Kind.Equals("table", StringComparison.OrdinalIgnoreCase) &&
                                        document.Id.Equals(tableName, StringComparison.OrdinalIgnoreCase));

    public async Task<IReadOnlyList<KnowledgeDocument>> GetByKindAsync(string kind, string? id, CancellationToken cancellationToken) =>
        (await LoadAsync(cancellationToken)).Documents
            .Where(document => document.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase) &&
                               (id is null || document.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(document => document.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public async Task<IReadOnlyList<KnowledgeSection>> ListSectionsAsync(string documentId, CancellationToken cancellationToken) =>
        (await LoadAsync(cancellationToken)).Sections
            .Where(section => section.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase))
            .ToArray();

    public async Task<KnowledgeSection?> GetSectionAsync(string documentId, string heading, CancellationToken cancellationToken) =>
        (await LoadAsync(cancellationToken)).Sections
            .FirstOrDefault(section => section.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase) &&
                                       section.Heading.Equals(heading, StringComparison.OrdinalIgnoreCase));

    public async Task<KnowledgePackStatus> GetStatusAsync(CancellationToken cancellationToken)
    {
        var knowledge = await LoadAsync(cancellationToken);
        return new(
            knowledge.Documents.Count > 0,
            knowledge.Documents.Count,
            knowledge.Sections.Count,
            knowledge.CreatedAtUtc,
            knowledge.FormatVersion,
            knowledge.Problem);
    }

    private async Task<LoadedKnowledge> LoadAsync(CancellationToken cancellationToken)
    {
        if (loaded is not null)
            return loaded;

        await loadGate.WaitAsync(cancellationToken);
        try
        {
            return loaded ??= await ReadPackAsync(cancellationToken);
        }
        finally
        {
            loadGate.Release();
        }
    }

    private async Task<LoadedKnowledge> ReadPackAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(options.PackPath))
            return LoadedKnowledge.Empty($"Knowledge pack was not found at {options.PackPath}.");

        if (!keyProvider.TryGetKey(out var key))
            return LoadedKnowledge.Empty("Knowledge decryption key is not available in the process environment.");

        KnowledgePack pack;
        try
        {
            pack = KnowledgePackCodec.Decrypt(await File.ReadAllBytesAsync(options.PackPath, cancellationToken), key);
        }
        catch (Exception exception) when (exception is CryptographicException or InvalidDataException)
        {
            // The pack is authenticated: a failure here means a wrong key or a tampered file, never partial data.
            logger.LogError(exception, "Knowledge pack could not be decrypted.");
            return LoadedKnowledge.Empty("Knowledge pack could not be decrypted; the key or the file is wrong.");
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }

        var documents = pack.Documents
            .Where(document => document.Exposure.Equals("model", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var sections = documents.SelectMany(KnowledgeChunker.Split).ToArray();

        logger.LogInformation(
            "Knowledge pack loaded: {DocumentCount} documents, {SectionCount} sections.",
            documents.Length, sections.Length);

        return new(documents, sections, KnowledgeIndex.Build(documents), pack.CreatedAtUtc, pack.FormatVersion, null);
    }

    private sealed record LoadedKnowledge(
        IReadOnlyList<KnowledgeDocument> Documents,
        IReadOnlyList<KnowledgeSection> Sections,
        KnowledgeIndex Index,
        DateTimeOffset CreatedAtUtc,
        int FormatVersion,
        string? Problem)
    {
        public static LoadedKnowledge Empty(string problem) =>
            new([], [], KnowledgeIndex.Build([]), DateTimeOffset.MinValue, 0, problem);
    }
}
