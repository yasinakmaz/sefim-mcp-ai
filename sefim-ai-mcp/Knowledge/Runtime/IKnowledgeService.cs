using SefimMcp.Knowledge.Models;

namespace SefimMcp.Knowledge.Runtime;

public interface IKnowledgeService
{
    Task<KnowledgeDocument?> GetApplicationOverviewAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<KnowledgeSearchHit>> SearchAsync(string query, int limit, CancellationToken cancellationToken);
    Task<IReadOnlyList<KnowledgeDocument>> ListTablesAsync(CancellationToken cancellationToken);
    Task<KnowledgeDocument?> GetTableAsync(string tableName, CancellationToken cancellationToken);
    Task<IReadOnlyList<KnowledgeDocument>> GetByKindAsync(string kind, string? id, CancellationToken cancellationToken);

    /// <summary>Returns one named section of a document, so a caller can read a heading without the whole file.</summary>
    Task<KnowledgeSection?> GetSectionAsync(string documentId, string heading, CancellationToken cancellationToken);
    Task<IReadOnlyList<KnowledgeSection>> ListSectionsAsync(string documentId, CancellationToken cancellationToken);
    Task<KnowledgePackStatus> GetStatusAsync(CancellationToken cancellationToken);
}

public sealed record KnowledgePackStatus(
    bool Loaded,
    int DocumentCount,
    int SectionCount,
    DateTimeOffset CreatedAtUtc,
    int FormatVersion,
    string? Problem);
