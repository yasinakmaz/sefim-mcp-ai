using Microsoft.Data.SqlClient;
using SqlService.Core.Interfaces;

namespace SefimMcp.Infrastructure.Metadata;

public sealed class SqlServerDatabaseMetadataService(IConnectionFactory connectionFactory) : IDatabaseMetadataService
{
    private const string MetadataQuery = """
        SELECT c.name, ty.name, c.max_length, c.precision, c.scale, c.is_nullable, c.is_identity, c.is_computed,
               dc.definition, CAST(CASE WHEN pk.column_id IS NULL THEN 0 ELSE 1 END AS bit), CAST(CASE WHEN uq.column_id IS NULL THEN 0 ELSE 1 END AS bit)
        FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id JOIN sys.columns c ON c.object_id=t.object_id
        JOIN sys.types ty ON ty.user_type_id=c.user_type_id
        LEFT JOIN sys.default_constraints dc ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
        LEFT JOIN (SELECT ic.object_id,ic.column_id FROM sys.key_constraints kc JOIN sys.index_columns ic ON ic.object_id=kc.parent_object_id AND ic.index_id=kc.unique_index_id WHERE kc.type='PK') pk ON pk.object_id=c.object_id AND pk.column_id=c.column_id
        LEFT JOIN (SELECT ic.object_id,ic.column_id FROM sys.key_constraints kc JOIN sys.index_columns ic ON ic.object_id=kc.parent_object_id AND ic.index_id=kc.unique_index_id WHERE kc.type='UQ') uq ON uq.object_id=c.object_id AND uq.column_id=c.column_id
        WHERE s.name=@schema AND t.name=@table ORDER BY c.column_id;
        SELECT fk.name, pc.name, rs.name + '.' + rt.name, rc.name
        FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id
        JOIN sys.tables pt ON pt.object_id=fk.parent_object_id JOIN sys.schemas ps ON ps.schema_id=pt.schema_id JOIN sys.columns pc ON pc.object_id=pt.object_id AND pc.column_id=fkc.parent_column_id
        JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id JOIN sys.schemas rs ON rs.schema_id=rt.schema_id JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
        WHERE ps.name=@schema AND pt.name=@table ORDER BY fk.name, fkc.constraint_column_id;
        """;

    public async Task<DatabaseTableMetadata?> DescribeTableAsync(string schema, string table, CancellationToken cancellationToken)
    {
        if (!IsIdentifier(schema) || !IsIdentifier(table))
            throw new ArgumentException("Schema and table names must be SQL identifiers.");

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(MetadataQuery, connection) { CommandTimeout = Math.Min(connectionFactory.GetCommandTimeout(), 30) };
        command.Parameters.AddWithValue("@schema", schema);
        command.Parameters.AddWithValue("@table", table);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var columns = new List<DatabaseColumnMetadata>();
        while (await reader.ReadAsync(cancellationToken))
            columns.Add(new(reader.GetString(0), reader.GetString(1), reader.GetInt16(2), reader.GetByte(3), reader.GetByte(4), reader.GetBoolean(5), reader.GetBoolean(6), reader.GetBoolean(7), reader.IsDBNull(8) ? null : reader.GetString(8), reader.GetBoolean(9), reader.GetBoolean(10)));
        if (columns.Count == 0)
            return null;
        var foreignKeys = new List<ForeignKeyMetadata>();
        await reader.NextResultAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            foreignKeys.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        return new(schema, table, columns, foreignKeys);
    }

    private static bool IsIdentifier(string value) => !string.IsNullOrWhiteSpace(value) && value.All(c => char.IsLetterOrDigit(c) || c == '_');
}
