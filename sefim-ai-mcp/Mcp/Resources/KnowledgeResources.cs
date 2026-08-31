using System.ComponentModel;
using ModelContextProtocol.Server;
using SefimMcp.Knowledge.Runtime;

namespace SefimMcp.Mcp.Resources;

/// <summary>
/// Resources mirror the discovery tools for clients that browse resources.
/// Tools stay the primary path because resource support differs between MCP clients.
/// </summary>
[McpServerResourceType]
public sealed class KnowledgeResources(IKnowledgeService knowledgeService)
{
    [McpServerResource(UriTemplate = "sefim://overview", MimeType = "text/markdown")]
    [Description("Compact model-visible Şefim overview. Tool discovery remains the primary access method for compatibility.")]
    public async Task<string> Overview(CancellationToken cancellationToken) =>
        (await knowledgeService.GetApplicationOverviewAsync(cancellationToken))?.Body
        ?? "No model-visible application overview is packed.";

    [McpServerResource(UriTemplate = "sefim://schema/table/{schema}/{table}", MimeType = "text/markdown")]
    [Description("Model-visible semantic documentation for one allowlisted table. Live technical metadata is available through describe_table.")]
    public async Task<string> Table(string schema, string table, CancellationToken cancellationToken) =>
        (await knowledgeService.GetTableAsync($"{schema}.{table}", cancellationToken))?.Body
        ?? "This table is not documented for model exposure.";

    [McpServerResource(UriTemplate = "sefim://knowledge/{kind}/{id}", MimeType = "text/markdown")]
    [Description("Model-visible knowledge document by kind and identifier, matching the get_knowledge_document tool.")]
    public async Task<string> Document(string kind, string id, CancellationToken cancellationToken) =>
        (await knowledgeService.GetByKindAsync(kind, id, cancellationToken)).FirstOrDefault()?.Body
        ?? $"No model-visible '{kind}' document with identifier '{id}'.";

    [McpServerResource(UriTemplate = "sefim://knowledge/status", MimeType = "text/plain")]
    [Description("Diagnostic view of the loaded knowledge pack: document and section counts, and any load problem.")]
    public async Task<string> Status(CancellationToken cancellationToken)
    {
        var status = await knowledgeService.GetStatusAsync(cancellationToken);
        return status.Problem is not null
            ? $"Knowledge pack not loaded: {status.Problem}"
            : $"Knowledge pack v{status.FormatVersion} created {status.CreatedAtUtc:u}: {status.DocumentCount} documents, {status.SectionCount} sections.";
    }
}
