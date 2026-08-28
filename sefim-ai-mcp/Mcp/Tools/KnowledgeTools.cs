using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using SefimMcp.Infrastructure.Metadata;
using SefimMcp.Knowledge.Runtime;
using SefimMcp.Mcp.Dtos;

namespace SefimMcp.Mcp.Tools;

[McpServerToolType]
public sealed class KnowledgeTools(IKnowledgeService knowledgeService, IDatabaseMetadataService databaseMetadataService)
{
    [McpServerTool(Name = "get_application_overview", ReadOnly = true, Destructive = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Returns the compact, model-visible description of the Şefim application. Use when the application itself is not understood.")]
    public async Task<KnowledgeDocumentResult> GetApplicationOverview(CancellationToken cancellationToken)
    {
        var document = await knowledgeService.GetApplicationOverviewAsync(cancellationToken);
        return document is null
            ? new(null, "not_documented", "No model-visible application overview is packed.")
            : new(document.Id, document.Status, document.Body);
    }

    [McpServerTool(Name = "search_application_knowledge", ReadOnly = true, Destructive = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Searches compact model-visible Şefim knowledge by business terms. Use before guessing a table, workflow, or rule name.")]
    public Task<IReadOnlyList<SefimMcp.Knowledge.Models.KnowledgeSearchHit>> SearchApplicationKnowledge(
        [Description("Business terms to search for, such as 'urun fiyat degistirme'.")] string query,
        [Description("Maximum hits, from 1 to 10.")] int limit = 5,
        CancellationToken cancellationToken = default) => knowledgeService.SearchAsync(query, Math.Clamp(limit, 1, 10), cancellationToken);

    [McpServerTool(Name = "list_documented_tables", ReadOnly = true, Destructive = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Lists only tables deliberately documented and allowed for model use. It does not enumerate the full database schema.")]
    public async Task<IReadOnlyList<DocumentedTableDto>> ListDocumentedTables(CancellationToken cancellationToken)
        => (await knowledgeService.ListTablesAsync(cancellationToken)).Select(document => new DocumentedTableDto(document.Id, document.Title, document.Status)).ToArray();

    [McpServerTool(Name = "describe_table", ReadOnly = true, Destructive = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Combines model-visible table semantics with compact live SQL Server metadata for one documented table. Use only after the table is known.")]
    public async Task<TableDescriptionDto> DescribeTable(
        [Description("Fully qualified documented table name in schema.table form, for example dbo.Product.")] string table,
        CancellationToken cancellationToken)
    {
        var parts = table.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            throw new McpException("Table must be provided as schema.table.");
        var document = await knowledgeService.GetTableAsync($"{parts[0]}.{parts[1]}", cancellationToken);
        if (document is null)
            return new(table, "not_documented", null, null, null, null, ["This table is not allowlisted for model exposure."]);

        var metadata = await databaseMetadataService.DescribeTableAsync(parts[0], parts[1], cancellationToken);
        return new(table, document.Status, ExtractSection(document.Body, "Purpose"), ExtractSection(document.Body, "Read Guidance"), ExtractSection(document.Body, "Write Guidance"), metadata, document.Status.Equals("draft", StringComparison.OrdinalIgnoreCase) ? ["Business documentation is draft; do not infer missing meanings."] : []);
    }

    [McpServerTool(Name = "get_business_rules", ReadOnly = true, Destructive = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Returns one specific model-visible business-rule document. Discover identifiers with search_application_knowledge; it never returns server-private policies.")]
    public Task<IReadOnlyList<SefimMcp.Knowledge.Models.KnowledgeDocument>> GetBusinessRules(
        [Description("Required business-rule identifier returned by search_application_knowledge.")] string ruleId,
        CancellationToken cancellationToken = default) => knowledgeService.GetByKindAsync("business-rule", ruleId, cancellationToken);

    [McpServerTool(Name = "get_workflow", ReadOnly = true, Destructive = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Returns one specific model-visible Şefim workflow document. Discover identifiers with search_application_knowledge.")]
    public Task<IReadOnlyList<SefimMcp.Knowledge.Models.KnowledgeDocument>> GetWorkflow(
        [Description("Required workflow identifier returned by search_application_knowledge.")] string workflowId,
        CancellationToken cancellationToken = default) => knowledgeService.GetByKindAsync("workflow", workflowId, cancellationToken);

    private static string? ExtractSection(string body, string heading)
    {
        var lines = body.Split('\n');
        var index = Array.FindIndex(lines, line => line.Trim().Equals($"## {heading}", StringComparison.OrdinalIgnoreCase));
        if (index < 0) return null;
        return string.Join('\n', lines.Skip(index + 1).TakeWhile(line => !line.StartsWith("## ", StringComparison.Ordinal))).Trim();
    }
}

public sealed record KnowledgeDocumentResult(string? Id, string Status, string Content);
