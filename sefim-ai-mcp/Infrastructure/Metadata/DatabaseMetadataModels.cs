namespace SefimMcp.Infrastructure.Metadata;

public sealed record DatabaseColumnMetadata(string Name, string SqlType, short MaxLength, byte Precision, byte Scale, bool IsNullable, bool IsIdentity, bool IsComputed, string? DefaultValue, bool IsPrimaryKey, bool IsUnique);
public sealed record ForeignKeyMetadata(string Name, string Column, string ReferencedTable, string ReferencedColumn);
public sealed record DatabaseTableMetadata(string Schema, string Name, IReadOnlyList<DatabaseColumnMetadata> Columns, IReadOnlyList<ForeignKeyMetadata> ForeignKeys);

public interface IDatabaseMetadataService
{
    Task<DatabaseTableMetadata?> DescribeTableAsync(string schema, string table, CancellationToken cancellationToken);
}
