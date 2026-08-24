using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SqlService.Core.Interfaces;
using SqlService.Core.Sprocs;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SqlService.Infrastructure.Sprocs;

/// <summary>
/// Stored Procedure service implementation
/// AOT-Compatible: Zero reflection, EntityMapper kullanır
/// </summary>
public sealed class SprocService : ISprocService
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IEntityMapper _entityMapper;
    private readonly ILogger<SprocService>? _logger;

    public SprocService(
        IConnectionFactory connectionFactory,
        IEntityMapper entityMapper,
        ILogger<SprocService>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(entityMapper);

        _connectionFactory = connectionFactory;
        _entityMapper = entityMapper;
        _logger = logger;
    }

    // ================== SINGLE RESULT SET ==================

    /// <inheritdoc/>
    public async Task<List<T>> QueryListAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sprocName);

        var stopwatch = Stopwatch.StartNew();
        var results = new List<T>();

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = CreateCommand(connection, sprocName, parameters);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(_entityMapper.MapFromReader<T>(reader));
        }

        stopwatch.Stop();
        _logger?.LogDebug(
            "Sproc {SprocName} returned {Count} rows in {ElapsedMs}ms",
            sprocName, results.Count, stopwatch.ElapsedMilliseconds);

        return results;
    }

    /// <inheritdoc/>
    public async Task<T?> QuerySingleAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sprocName);

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = CreateCommand(connection, sprocName, parameters);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return _entityMapper.MapFromReader<T>(reader);
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<T> QuerySingleRequiredAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var result = await QuerySingleAsync<T>(sprocName, parameters, cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException(
                $"Stored procedure '{sprocName}' returned no results of type {typeof(T).Name}.");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<T?> QueryScalarAsync<T>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sprocName);

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = CreateCommand(connection, sprocName, parameters);

        var result = await command.ExecuteScalarAsync(cancellationToken);

        if (result == null || result == DBNull.Value)
            return default;

        return ConvertScalar<T>(result);
    }

    // ================== TWO RESULT SETS ==================

    /// <inheritdoc/>
    public async Task<(T1? First, T2? Second)> QueryAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T1,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T2>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default)
        where T1 : class
        where T2 : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sprocName);

        await using var reader = await QueryMultipleAsync(sprocName, parameters, cancellationToken);

        var first = await reader.ReadSingleAsync<T1>(cancellationToken);
        var second = await reader.ReadSingleAsync<T2>(cancellationToken);

        return (first, second);
    }

    /// <inheritdoc/>
    public async Task<(TScalar? First, List<TEntity> Second)> QueryScalarAndListAsync<TScalar,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] TEntity>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sprocName);

        var stopwatch = Stopwatch.StartNew();

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = CreateCommand(connection, sprocName, parameters);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        // İlk result set - Scalar (örn: TotalCount)
        TScalar? scalar = default;
        if (await reader.ReadAsync(cancellationToken))
        {
            var value = reader.GetValue(0);
            if (value != null && value != DBNull.Value)
            {
                scalar = ConvertScalar<TScalar>(value);
            }
        }

        // İkinci result set'e geç
        await reader.NextResultAsync(cancellationToken);

        // İkinci result set - Liste
        var list = new List<TEntity>();
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(_entityMapper.MapFromReader<TEntity>(reader));
        }

        stopwatch.Stop();
        _logger?.LogDebug(
            "Sproc {SprocName} returned scalar + {Count} rows in {ElapsedMs}ms",
            sprocName, list.Count, stopwatch.ElapsedMilliseconds);

        return (scalar, list);
    }

    // ================== THREE RESULT SETS ==================

    /// <inheritdoc/>
    public async Task<(T1? First, T2? Second, T3? Third)> QueryAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T1,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T2,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T3>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default)
        where T1 : class
        where T2 : class
        where T3 : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sprocName);

        await using var reader = await QueryMultipleAsync(sprocName, parameters, cancellationToken);

        var first = await reader.ReadSingleAsync<T1>(cancellationToken);
        var second = await reader.ReadSingleAsync<T2>(cancellationToken);
        var third = await reader.ReadSingleAsync<T3>(cancellationToken);

        return (first, second, third);
    }

    // ================== FOUR RESULT SETS ==================

    /// <inheritdoc/>
    public async Task<(T1? First, T2? Second, T3? Third, T4? Fourth)> QueryAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T1,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T2,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T3,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T4>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default)
        where T1 : class
        where T2 : class
        where T3 : class
        where T4 : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sprocName);

        await using var reader = await QueryMultipleAsync(sprocName, parameters, cancellationToken);

        var first = await reader.ReadSingleAsync<T1>(cancellationToken);
        var second = await reader.ReadSingleAsync<T2>(cancellationToken);
        var third = await reader.ReadSingleAsync<T3>(cancellationToken);
        var fourth = await reader.ReadSingleAsync<T4>(cancellationToken);

        return (first, second, third, fourth);
    }

    // ================== MANUAL READER ==================

    /// <inheritdoc/>
    public async Task<ISprocReader> QueryMultipleAsync(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sprocName);

        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var command = CreateCommand(connection, sprocName, parameters);

        try
        {
            var reader = await command.ExecuteReaderAsync(cancellationToken);
            return new SprocReader(connection, command, reader, _entityMapper, parameters);
        }
        catch
        {
            await command.DisposeAsync();
            await connection.DisposeAsync();
            throw;
        }
    }

    // ================== NON-QUERY ==================

    /// <inheritdoc/>
    public async Task<int> ExecuteAsync(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sprocName);

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = CreateCommand(connection, sprocName, parameters);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);

        _logger?.LogDebug(
            "Sproc {SprocName} affected {Count} rows",
            sprocName, affected);

        return affected;
    }

    /// <inheritdoc/>
    public async Task<(int AffectedRows, IReadOnlyDictionary<string, object?> Outputs)> ExecuteWithOutputAsync(
        string sprocName,
        SprocParams parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sprocName);
        ArgumentNullException.ThrowIfNull(parameters);

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = CreateCommand(connection, sprocName, parameters);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        var outputs = parameters.GetOutputValues(command);

        _logger?.LogDebug(
            "Sproc {SprocName} affected {Count} rows with {OutputCount} output parameters",
            sprocName, affected, outputs.Count);

        return (affected, outputs);
    }

    // ================== HELPER METHODS ==================

    /// <summary>
    /// SqlCommand oluşturur
    /// </summary>
    private SqlCommand CreateCommand(SqlConnection connection, string sprocName, SprocParams? parameters)
    {
        var command = new SqlCommand(sprocName, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = _connectionFactory.GetCommandTimeout()
        };

        parameters?.ApplyTo(command);

        return command;
    }

    /// <summary>
    /// Scalar değer dönüştürme - AOT uyumlu
    /// </summary>
    private static T? ConvertScalar<T>(object value)
    {
        if (value == null || value == DBNull.Value)
            return default;

        var targetType = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Direct type match
        if (value.GetType() == underlyingType)
            return (T)value;

        // Common conversions (AOT-safe)
        if (underlyingType == typeof(int))
            return (T)(object)Convert.ToInt32(value);

        if (underlyingType == typeof(long))
            return (T)(object)Convert.ToInt64(value);

        if (underlyingType == typeof(decimal))
            return (T)(object)Convert.ToDecimal(value);

        if (underlyingType == typeof(double))
            return (T)(object)Convert.ToDouble(value);

        if (underlyingType == typeof(float))
            return (T)(object)Convert.ToSingle(value);

        if (underlyingType == typeof(bool))
            return (T)(object)Convert.ToBoolean(value);

        if (underlyingType == typeof(string))
            return (T)(object)value.ToString()!;

        if (underlyingType == typeof(Guid))
        {
            if (value is Guid g)
                return (T)(object)g;
            if (value is string s)
                return (T)(object)Guid.Parse(s);
            if (value is byte[] bytes)
                return (T)(object)new Guid(bytes);
        }

        if (underlyingType == typeof(DateTime))
            return (T)(object)Convert.ToDateTime(value);

        if (underlyingType == typeof(DateTimeOffset))
        {
            if (value is DateTimeOffset dto)
                return (T)(object)dto;
            if (value is DateTime dt)
                return (T)(object)new DateTimeOffset(dt);
        }

        if (underlyingType == typeof(byte[]))
            return (T)value;

        // Fallback
        return (T)Convert.ChangeType(value, underlyingType);
    }
}
