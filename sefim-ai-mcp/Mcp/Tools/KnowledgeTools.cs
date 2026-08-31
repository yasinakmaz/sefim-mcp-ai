using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using SefimMcp.Infrastructure.Metadata;
using SefimMcp.Knowledge.Models;
using SefimMcp.Knowledge.Runtime;
using SefimMcp.Mcp.Dtos;

namespace SefimMcp.Mcp.Tools;

/// <summary>
/// Discovery surface over the encrypted knowledge pack.
/// The intended path is search first, then fetch one document or one section - never a bulk dump, because
/// context spent on unrelated documentation is context the model no longer has for the actual task.
/// </summary>
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
    [Description("Full-text search over Şefim business knowledge. Turkish-aware: 'urun', 'Ürün' and 'ürünün' match the same documents. Returns document identifiers and section headings; fetch the full text with get_knowledge_document. Use before guessing a table, workflow or rule name.")]
    public Task<IReadOnlyList<KnowledgeSearchHit>> SearchApplicationKnowledge(
        [Description("Business terms to search for, such as 'urun fiyat degistirme' or 'adisyon kapatma'.")] string query,
        [Description("Maximum hits, from 1 to 10.")] int limit = 5,
        CancellationToken cancellationToken = default) =>
        knowledgeService.SearchAsync(query, Math.Clamp(limit, 1, 10), cancellationToken);

    [McpServerTool(Name = "list_knowledge_documents", ReadOnly = true, Destructive = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Lists documented knowledge with identifiers, summaries and section headings, without returning body text. Use to see what exists before fetching. Optionally filter by kind: application, glossary, guidance, table, workflow, business-rule.")]
    public async Task<IReadOnlyList<KnowledgeIndexEntryDto>> ListKnowledgeDocuments(
        [Description("Optional kind filter. Leave empty to list every documented kind.")] string? kind = null,
        CancellationToken cancellationToken = default)
    {
        var kinds = string.IsNullOrWhiteSpace(kind)
            ? new[] { "application", "glossary", "guidance", "table", "workflow", "business-rule" }
            : [kind];

        var entries = new List<KnowledgeIndexEntryDto>();
        foreach (var currentKind in kinds)
        {
            foreach (var document in await knowledgeService.GetByKindAsync(currentKind, null, cancellationToken))
            {
                var sections = (await knowledgeService.ListSectionsAsync(document.Id, cancellationToken))
                    .Select(section => section.Heading)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                entries.Add(new(document.Kind, document.Id, document.Title, document.Status, document.Summary, sections));
            }
        }

        return entries;
    }

    [McpServerTool(Name = "get_knowledge_document", ReadOnly = true, Destructive = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Returns one model-visible knowledge document by kind and identifier. Pass a section heading to retrieve only that section instead of the whole document. Identifiers come from search_application_knowledge or list_knowledge_documents.")]
    public async Task<KnowledgeDocumentResult> GetKnowledgeDocument(
        [Description("Document kind: application, glossary, guidance, table, workflow or business-rule.")] string kind,
        [Description("Document identifier, such as 'ai-guidance' or 'dbo.Product'.")] string id,
        [Description("Optional section heading, such as 'Write Guidance'. Reduces the returned text to that section only.")] string? section = null,
        CancellationToken cancellationToken = default)
    {
        var document = (await knowledgeService.GetByKindAsync(kind, id, cancellationToken)).FirstOrDefault();
        if (document is null)
            return new(null, "not_documented", $"No model-visible '{kind}' document with identifier '{id}'.");

        if (string.IsNullOrWhiteSpace(section))
            return new(document.Id, document.Status, document.Body);

        var match = await knowledgeService.GetSectionAsync(document.Id, section, cancellationToken);
        return match is null
            ? new(document.Id, document.Status, $"Section '{section}' does not exist in '{document.Id}'.")
            : new(document.Id, document.Status, $"## {match.Heading}\n\n{match.Text}");
    }

    [McpServerTool(Name = "lookup_glossary_terms", ReadOnly = true, Destructive = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Resolves Şefim-specific business terms to their documented meaning. Use when a user word such as 'adisyon', 'kuver', 'marşlama' or 'zayi' is not certainly understood, instead of inferring it from a table or column name.")]
    public async Task<IReadOnlyList<GlossaryTermDto>> LookupGlossaryTerms(
        [Description("One or more terms to resolve.")] string[] terms,
        CancellationToken cancellationToken = default)
    {
        var results = new List<GlossaryTermDto>();
        foreach (var glossary in await knowledgeService.GetByKindAsync("glossary", null, cancellationToken))
        {
            var sections = await knowledgeService.ListSectionsAsync(glossary.Id, cancellationToken);
            foreach (var term in terms.Where(term => !string.IsNullOrWhiteSpace(term)))
            {
                var folded = KnowledgeText.Fold(term);
                foreach (var section in sections)
                {
                    var headingFolded = KnowledgeText.Fold(section.Heading);
                    var exact = headingFolded.Equals(folded, StringComparison.Ordinal);
                    if (!exact && !headingFolded.Contains(folded, StringComparison.Ordinal))
                        continue;

                    results.Add(new(section.Heading, section.Text, exact));
                }
            }
        }

        return results
            .OrderByDescending(result => result.ExactMatch)
            .DistinctBy(result => result.Term, StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToArray();
    }

    [McpServerTool(Name = "list_documented_tables", ReadOnly = true, Destructive = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Lists only the tables deliberately documented and allowed for model use. It does not enumerate the full database schema.")]
    public async Task<IReadOnlyList<DocumentedTableDto>> ListDocumentedTables(CancellationToken cancellationToken)
        => (await knowledgeService.ListTablesAsync(cancellationToken))
            .Select(document => new DocumentedTableDto(document.Id, document.Title, document.Status))
            .ToArray();

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
        return new(
            table,
            document.Status,
            KnowledgeMarkdown.ExtractSection(document.Body, "Purpose"),
            KnowledgeMarkdown.ExtractSection(document.Body, "Read Guidance"),
            KnowledgeMarkdown.ExtractSection(document.Body, "Write Guidance"),
            metadata,
            document.Status.Equals("draft", StringComparison.OrdinalIgnoreCase)
                ? ["Business documentation is draft; do not infer missing meanings."]
                : []);
    }

    [McpServerTool(Name = "get_business_rules", ReadOnly = true, Destructive = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Returns one specific model-visible business-rule document. Discover identifiers with search_application_knowledge; it never returns server-private policies.")]
    public Task<IReadOnlyList<KnowledgeDocument>> GetBusinessRules(
        [Description("Required business-rule identifier returned by search_application_knowledge.")] string ruleId,
        CancellationToken cancellationToken = default) =>
        knowledgeService.GetByKindAsync("business-rule", ruleId, cancellationToken);

    [McpServerTool(Name = "get_workflow", ReadOnly = true, Destructive = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Returns one specific model-visible Şefim workflow document. Discover identifiers with search_application_knowledge.")]
    public Task<IReadOnlyList<KnowledgeDocument>> GetWorkflow(
        [Description("Required workflow identifier returned by search_application_knowledge.")] string workflowId,
        CancellationToken cancellationToken = default) =>
        knowledgeService.GetByKindAsync("workflow", workflowId, cancellationToken);
}

public sealed record KnowledgeDocumentResult(string? Id, string Status, string Content);
