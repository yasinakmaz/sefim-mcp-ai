namespace SefimMcp.Knowledge.Models;

public sealed record KnowledgeDocument(
    string Kind,
    string Id,
    string Status,
    string Exposure,
    string Title,
    string Body,
    IReadOnlyDictionary<string, string> Metadata,
    string SourceName);

public sealed record KnowledgeSearchHit(
    string Kind,
    string Id,
    string Title,
    string Status,
    string Summary,
    int Score);

public sealed record KnowledgePack(IReadOnlyList<KnowledgeDocument> Documents, DateTimeOffset CreatedAtUtc, int FormatVersion = 1);

public sealed record KnowledgeValidationIssue(string SourceName, string Code, string Message);

public sealed record KnowledgeValidationResult(bool IsValid, IReadOnlyList<KnowledgeValidationIssue> Issues);

public sealed record KnowledgePackOptions(string PackPath, string? KeyEnvironmentVariable = "SEFIM_KNOWLEDGE_KEY");
