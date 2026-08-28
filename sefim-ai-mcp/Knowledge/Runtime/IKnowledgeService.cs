using SefimMcp.Knowledge.Models;

namespace SefimMcp.Knowledge.Runtime;

public interface IKnowledgeService
{
    Task<KnowledgeDocument?> GetApplicationOverviewAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<KnowledgeSearchHit>> SearchAsync(string query, int limit, CancellationToken cancellationToken);
    Task<IReadOnlyList<KnowledgeDocument>> ListTablesAsync(CancellationToken cancellationToken);
    Task<KnowledgeDocument?> GetTableAsync(string tableName, CancellationToken cancellationToken);
    Task<IReadOnlyList<KnowledgeDocument>> GetByKindAsync(string kind, string? id, CancellationToken cancellationToken);
}
