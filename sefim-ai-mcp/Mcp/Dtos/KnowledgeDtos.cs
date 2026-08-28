using SefimMcp.Infrastructure.Metadata;

namespace SefimMcp.Mcp.Dtos;

public sealed record DocumentedTableDto(string Id, string Title, string DocumentationStatus);
public sealed record TableDescriptionDto(string Table, string DocumentationStatus, string? Purpose, string? ReadGuidance, string? WriteGuidance, DatabaseTableMetadata? TechnicalSchema, IReadOnlyList<string> Warnings);
public sealed record OperationChallengeDto(string Operation, string Target, string Challenge, DateTimeOffset ExpiresAtUtc);
