using System.ComponentModel;
using ModelContextProtocol.Server;
using SefimMcp.Knowledge.Runtime;

namespace SefimMcp.Mcp.Resources;

[McpServerResourceType]
public sealed class KnowledgeResources(IKnowledgeService knowledgeService)
{
    [McpServerResource(UriTemplate = "sefim://overview", MimeType = "text/markdown")]
    [Description("Compact model-visible Şefim overview. Tool discovery remains the primary access method for compatibility.")]
    public async Task<string> Overview(CancellationToken cancellationToken)
        => (await knowledgeService.GetApplicationOverviewAsync(cancellationToken))?.Body ?? "No model-visible application overview is packed.";

    [McpServerResource(UriTemplate = "sefim://schema/table/{schema}/{table}", MimeType = "text/markdown")]
    [Description("Model-visible semantic documentation for one allowlisted table. Live technical metadata is available through describe_table.")]
    public async Task<string> Table(string schema, string table, CancellationToken cancellationToken)
        => (await knowledgeService.GetTableAsync($"{schema}.{table}", cancellationToken))?.Body ?? "This table is not documented for model exposure.";
}
