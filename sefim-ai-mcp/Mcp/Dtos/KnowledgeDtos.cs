using SefimMcp.Infrastructure.Metadata;
using SefimMcp.Knowledge.Models;

namespace SefimMcp.Mcp.Dtos;

public sealed record DocumentedTableDto(string Id, string Title, string DocumentationStatus);

public sealed record TableDescriptionDto(
    string Table,
    string DocumentationStatus,
    string? Purpose,
    string? ReadGuidance,
    string? WriteGuidance,
    DatabaseTableMetadata? TechnicalSchema,
    IReadOnlyList<string> Warnings);

public sealed record OperationChallengeDto(string Operation, string Target, string Challenge, DateTimeOffset ExpiresAtUtc);

/// <summary>Compact map entry used by list tools: enough for the model to choose, too little to be expensive.</summary>
public sealed record KnowledgeIndexEntryDto(string Kind, string Id, string Title, string Status, string? Summary, IReadOnlyList<string> Sections);

public sealed record KnowledgeSectionDto(string DocumentId, string Heading, string Content);

public sealed record GlossaryTermDto(string Term, string Definition, bool ExactMatch);
