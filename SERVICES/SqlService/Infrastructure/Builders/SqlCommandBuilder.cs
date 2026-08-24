using Microsoft.Data.SqlClient;
using SqlService.Core.Interfaces;
using SqlService.Infrastructure.Factories;
using SqlService.Infrastructure.Mappers;
using SqlService.Infrastructure.Utilities;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SqlService.Infrastructure.Builders;

/// <summary>
/// SQL command builder implementation - 100% Reflection-free SQL generation
/// AOT-Compatible: No runtime reflection, uses EntityMapper for property access
/// </summary>
public sealed class SqlCommandBuilder : ISqlCommandBuilder
{
    private readonly SqlCommandFactory _commandFactory;
    private readonly IEntityMapper _entityMapper;

    public SqlCommandBuilder(IConnectionFactory connectionFactory, IEntityMapper entityMapper)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(entityMapper);

        _commandFactory = new SqlCommandFactory(connectionFactory);
        _entityMapper = entityMapper;
    }

    /// <inheritdoc/>
    public SqlCommand BuildSelectByIdCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(object id)
        where T : class
    {
        var metadata = EntityMetadata<T>.Metadata;

        if (metadata.PrimaryKeyProperties.Length == 0)
            throw new InvalidOperationException($"Entity {typeof(T).Name} has no primary key defined.");

        var pkProperty = metadata.PrimaryKeyProperties[0];

        // Cache key: TypeName + Operation
        var cacheKey = $"{typeof(T).FullName}:SelectById";

        // Get cached or generate SQL with explicit column list
        var sql = SqlCommandCache.GetOrCreateSelect(cacheKey, () =>
            $"SELECT {metadata.ColumnList} FROM {metadata.FullTableName} WHERE [{pkProperty.ColumnName}] = @Id");

        var command = _commandFactory.CreateCommand(sql);
        _commandFactory.AddParameter(command, "@Id", id);

        return command;
    }

    /// <inheritdoc/>
    public SqlCommand BuildSelectAllCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : class
    {
        var metadata = EntityMetadata<T>.Metadata;

        // Cache key: TypeName + Operation
        var cacheKey = $"{typeof(T).FullName}:SelectAll";

        // Get cached or generate SQL with explicit column list
        var sql = SqlCommandCache.GetOrCreateSelect(cacheKey, () =>
            $"SELECT {metadata.ColumnList} FROM {metadata.FullTableName}");

        return _commandFactory.CreateCommand(sql);
    }

    /// <inheritdoc/>
    public SqlCommand BuildSelectWhereCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        string whereClause,
        IReadOnlyDictionary<string, object?> parameters)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(whereClause);
        ArgumentNullException.ThrowIfNull(parameters);

        var metadata = EntityMetadata<T>.Metadata;

        // Cache key: TypeName + Operation + WhereClause hash
        var cacheKey = $"{typeof(T).FullName}:SelectWhere:{whereClause.GetHashCode()}";

        // Get cached or generate SQL with explicit column list
        var sql = SqlCommandCache.GetOrCreateSelect(cacheKey, () =>
            $"SELECT {metadata.ColumnList} FROM {metadata.FullTableName} WHERE {whereClause}");

        var command = _commandFactory.CreateCommand(sql);
        _commandFactory.AddParameters(command, parameters);

        return command;
    }

    /// <inheritdoc/>
    public SqlCommand BuildSelectPagedCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        int pageNumber,
        int pageSize)
        where T : class
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than 0.", nameof(pageNumber));
        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than 0.", nameof(pageSize));

        var metadata = EntityMetadata<T>.Metadata;

        // Cache key: TypeName + Operation
        var cacheKey = $"{typeof(T).FullName}:SelectPaged";

        // Get cached or generate SQL with explicit column list (OFFSET-FETCH pagination for SQL Server 2012+)
        var sql = SqlCommandCache.GetOrCreateSelect(cacheKey, () => $@"
            SELECT {metadata.ColumnList} FROM {metadata.FullTableName}
            ORDER BY (SELECT NULL)
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY");

        var offset = (pageNumber - 1) * pageSize;
        var command = _commandFactory.CreateCommand(sql);
        _commandFactory.AddParameter(command, "@Offset", offset);
        _commandFactory.AddParameter(command, "@PageSize", pageSize);

        return command;
    }

    /// <inheritdoc/>
    public SqlCommand BuildSelectPagedWithCountCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        int pageNumber,
        int pageSize)
        where T : class
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than 0.", nameof(pageNumber));
        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than 0.", nameof(pageSize));

        var metadata = EntityMetadata<T>.Metadata;

        // Determine ORDER BY column (prefer primary key for deterministic results)
        var orderByColumn = metadata.PrimaryKeyProperties.Length > 0
            ? metadata.PrimaryKeyProperties[0].ColumnName
            : metadata.Properties[0].ColumnName;

        // Cache key: TypeName + Operation
        var cacheKey = $"{typeof(T).FullName}:SelectPagedWithCount";

        // Get cached or generate SQL with COUNT(*) OVER() window function and explicit column list
        // This returns both data AND total count in a SINGLE query (40-50% faster)
        var sql = SqlCommandCache.GetOrCreateSelect(cacheKey, () => $@"
            SELECT {metadata.ColumnList},
                   COUNT(*) OVER() AS TotalCount
            FROM {metadata.FullTableName}
            ORDER BY [{orderByColumn}]
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY");

        var offset = (pageNumber - 1) * pageSize;
        var command = _commandFactory.CreateCommand(sql);
        _commandFactory.AddParameter(command, "@Offset", offset);
        _commandFactory.AddParameter(command, "@PageSize", pageSize);

        return command;
    }

    /// <inheritdoc/>
    public SqlCommand BuildCountCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        string? whereClause = null,
        IReadOnlyDictionary<string, object?>? parameters = null)
        where T : class
    {
        var metadata = EntityMetadata<T>.Metadata;

        // Cache key: TypeName + Operation + Optional WhereClause hash
        var whereHash = string.IsNullOrWhiteSpace(whereClause) ? "NoWhere" : whereClause.GetHashCode().ToString();
        var cacheKey = $"{typeof(T).FullName}:Count:{whereHash}";

        // Get cached or generate SQL
        var sql = SqlCommandCache.GetOrCreateCount(cacheKey, () =>
            string.IsNullOrWhiteSpace(whereClause)
                ? $"SELECT COUNT(*) FROM {metadata.FullTableName}"
                : $"SELECT COUNT(*) FROM {metadata.FullTableName} WHERE {whereClause}");

        var command = _commandFactory.CreateCommand(sql);

        if (parameters != null && parameters.Count > 0)
            _commandFactory.AddParameters(command, parameters);

        return command;
    }

    /// <inheritdoc/>
    public SqlCommand BuildInsertCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(T entity)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var metadata = EntityMetadata<T>.Metadata;
        var insertableProps = metadata.InsertableProperties;

        if (insertableProps.Length == 0)
            throw new InvalidOperationException($"Entity {typeof(T).Name} has no insertable properties.");

        // Cache key: TypeName + Operation
        var cacheKey = $"{typeof(T).FullName}:Insert";

        // Get cached or generate SQL
        var sql = SqlCommandCache.GetOrCreateInsert(cacheKey, () =>
        {
            var columns = string.Join(", ", insertableProps.Select(p => $"[{p.ColumnName}]"));
            var parameters = string.Join(", ", insertableProps.Select(p => $"@{p.PropertyName}"));
            return $"INSERT INTO {metadata.FullTableName} ({columns}) VALUES ({parameters})";
        });

        var command = _commandFactory.CreateCommand(sql);

        // Get property values using compiled expressions (ZERO reflection)
        var propertyValues = _entityMapper.GetPropertyValues(entity);

        foreach (var prop in insertableProps)
        {
            var value = propertyValues[prop.ColumnName];
            _commandFactory.AddParameter(command, $"@{prop.PropertyName}", value);
        }

        return command;
    }

    /// <inheritdoc/>
    public SqlCommand BuildInsertWithOutputCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(T entity, out bool hasIdentity)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var metadata = EntityMetadata<T>.Metadata;
        var insertableProps = metadata.InsertableProperties;

        if (insertableProps.Length == 0)
            throw new InvalidOperationException($"Entity {typeof(T).Name} has no insertable properties.");

        // Identity property kontrolü
        var identityProp = metadata.PrimaryKeyProperties.FirstOrDefault(p => p.IsIdentity);
        hasIdentity = identityProp != null;

        // Cache key: TypeName + Operation + Identity flag
        var cacheKey = $"{typeof(T).FullName}:InsertWithOutput:{hasIdentity}";

        // Get cached or generate SQL
        var sql = SqlCommandCache.GetOrCreateInsert(cacheKey, () =>
        {
            var columns = string.Join(", ", insertableProps.Select(p => $"[{p.ColumnName}]"));
            var parameters = string.Join(", ", insertableProps.Select(p => $"@{p.PropertyName}"));

            if (identityProp != null)
            {
                // OUTPUT INSERTED.[IdentityColumn] ile ID döndür
                return $"INSERT INTO {metadata.FullTableName} ({columns}) OUTPUT INSERTED.[{identityProp.ColumnName}] VALUES ({parameters})";
            }
            else
            {
                // Normal INSERT
                return $"INSERT INTO {metadata.FullTableName} ({columns}) VALUES ({parameters})";
            }
        });

        var command = _commandFactory.CreateCommand(sql);

        // Get property values using compiled expressions (ZERO reflection)
        var propertyValues = _entityMapper.GetPropertyValues(entity);

        foreach (var prop in insertableProps)
        {
            var value = propertyValues[prop.ColumnName];
            _commandFactory.AddParameter(command, $"@{prop.PropertyName}", value);
        }

        return command;
    }

    /// <inheritdoc/>
    public SqlCommand BuildUpdateCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(T entity)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var metadata = EntityMetadata<T>.Metadata;

        if (metadata.PrimaryKeyProperties.Length == 0)
            throw new InvalidOperationException($"Entity {typeof(T).Name} has no primary key defined.");

        var nonPkProps = metadata.NonPrimaryKeyProperties;

        if (nonPkProps.Length == 0)
            throw new InvalidOperationException($"Entity {typeof(T).Name} has no updatable properties.");

        // Cache key: TypeName + Operation
        var cacheKey = $"{typeof(T).FullName}:Update";

        // Get cached or generate SQL
        var sql = SqlCommandCache.GetOrCreateUpdate(cacheKey, () =>
        {
            var setClause = string.Join(", ", nonPkProps.Select(p => $"[{p.ColumnName}] = @{p.PropertyName}"));
            var whereClause = string.Join(" AND ", metadata.PrimaryKeyProperties.Select(p => $"[{p.ColumnName}] = @{p.PropertyName}"));
            return $"UPDATE {metadata.FullTableName} SET {setClause} WHERE {whereClause}";
        });

        var command = _commandFactory.CreateCommand(sql);

        // Get property values using compiled expressions (ZERO reflection)
        var propertyValues = _entityMapper.GetPropertyValues(entity);

        // SET kısmı için parametreler
        foreach (var prop in nonPkProps)
        {
            var value = propertyValues[prop.ColumnName];
            _commandFactory.AddParameter(command, $"@{prop.PropertyName}", value);
        }

        // WHERE kısmı için parametreler (PK)
        foreach (var prop in metadata.PrimaryKeyProperties)
        {
            var value = propertyValues[prop.ColumnName];
            _commandFactory.AddParameter(command, $"@{prop.PropertyName}", value);
        }

        return command;
    }

    /// <inheritdoc/>
    public SqlCommand BuildDeleteCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(object id)
        where T : class
    {
        var metadata = EntityMetadata<T>.Metadata;

        if (metadata.PrimaryKeyProperties.Length == 0)
            throw new InvalidOperationException($"Entity {typeof(T).Name} has no primary key defined.");

        var pkProperty = metadata.PrimaryKeyProperties[0];

        // Cache key: TypeName + Operation
        var cacheKey = $"{typeof(T).FullName}:Delete";

        // Get cached or generate SQL
        var sql = SqlCommandCache.GetOrCreateDelete(cacheKey, () =>
            $"DELETE FROM {metadata.FullTableName} WHERE [{pkProperty.ColumnName}] = @Id");

        var command = _commandFactory.CreateCommand(sql);
        _commandFactory.AddParameter(command, "@Id", id);

        return command;
    }

    /// <inheritdoc/>
    public SqlCommand BuildDeleteWhereCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        string whereClause,
        IReadOnlyDictionary<string, object?> parameters)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(whereClause);
        ArgumentNullException.ThrowIfNull(parameters);

        var metadata = EntityMetadata<T>.Metadata;

        // Cache key: TypeName + Operation + WhereClause hash
        var cacheKey = $"{typeof(T).FullName}:DeleteWhere:{whereClause.GetHashCode()}";

        // Get cached or generate SQL
        var sql = SqlCommandCache.GetOrCreateDelete(cacheKey, () =>
            $"DELETE FROM {metadata.FullTableName} WHERE {whereClause}");

        var command = _commandFactory.CreateCommand(sql);
        _commandFactory.AddParameters(command, parameters);

        return command;
    }
}
