namespace SefimMcp.Knowledge.Models;

public sealed record KnowledgeDocument(
    string Kind,
    string Id,
    string Status,
    string Exposure,
    string Title,
    string Body,
    IReadOnlyDictionary<string, string> Metadata,
    string SourceName,
    string? Summary = null,
    IReadOnlyList<string>? Aliases = null);

/// <summary>One retrievable slice of a document. Search works at this granularity so a hit points at a heading.</summary>
public sealed record KnowledgeSection(
    string DocumentId,
    string Kind,
    string DocumentTitle,
    string Heading,
    string Text);

public sealed record KnowledgeSearchHit(
    string Kind,
    string Id,
    string Title,
    string Status,
    string Summary,
    int Score,
    string? Section = null,
    string? Uri = null);

public sealed record KnowledgePack(
    IReadOnlyList<KnowledgeDocument> Documents,
    DateTimeOffset CreatedAtUtc,
    int FormatVersion = 2,
    string? SourceFingerprint = null);

public sealed record KnowledgeValidationIssue(string SourceName, string Code, string Message);

public sealed record KnowledgeValidationResult(bool IsValid, IReadOnlyList<KnowledgeValidationIssue> Issues);

public sealed record KnowledgePackOptions(string PackPath, string? KeyEnvironmentVariable = "SEFIM_KNOWLEDGE_KEY");
