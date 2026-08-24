using Microsoft.Data.SqlClient;
using SqlService.Core.Interfaces;
using SqlService.Core.Sprocs;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SqlService.Infrastructure.Sprocs;

/// <summary>
/// Stored Procedure result set reader implementation
/// AOT-Compatible: Zero reflection, EntityMapper kullanır
/// </summary>
internal sealed class SprocReader : ISprocReader
{
    private readonly SqlConnection _connection;
    private readonly SqlCommand _command;
    private readonly SqlDataReader _reader;
    private readonly IEntityMapper _entityMapper;
    private readonly SprocParams? _parameters;

    private bool _disposed;
    private bool _hasMoreResults = true;
    private Dictionary<string, object?>? _outputParameters;
    private int? _returnValue;

    internal SprocReader(
        SqlConnection connection,
        SqlCommand command,
        SqlDataReader reader,
        IEntityMapper entityMapper,
        SprocParams? parameters)
    {
        _connection = connection;
        _command = command;
        _reader = reader;
        _entityMapper = entityMapper;
        _parameters = parameters;
    }

    /// <inheritdoc/>
    public bool HasMoreResults => _hasMoreResults && !_reader.IsClosed;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> OutputParameters
    {
        get
        {
            EnsureOutputParametersRead();
            return _outputParameters!;
        }
    }

    /// <inheritdoc/>
    public int? ReturnValue
    {
        get
        {
            EnsureOutputParametersRead();
            return _returnValue;
        }
    }

    /// <inheritdoc/>
    public async Task<T?> ReadScalarAsync<T>(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!_hasMoreResults)
            return default;

        T? result = default;

        if (await _reader.ReadAsync(cancellationToken))
        {
            var value = _reader.GetValue(0);

            if (value != null && value != DBNull.Value)
            {
                result = ConvertScalar<T>(value);
            }
        }

        // Otomatik olarak bir sonraki result set'e geç
        _hasMoreResults = await _reader.NextResultAsync(cancellationToken);

        return result;
    }

    /// <inheritdoc/>
    public async Task<T?> ReadSingleAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        CancellationToken cancellationToken = default) where T : class
    {
        ThrowIfDisposed();

        if (!_hasMoreResults)
            return null;

        T? result = null;

        if (await _reader.ReadAsync(cancellationToken))
        {
            result = _entityMapper.MapFromReader<T>(_reader);
        }

        // Kalan satırları atla ve bir sonraki result set'e geç
        while (await _reader.ReadAsync(cancellationToken)) { }
        _hasMoreResults = await _reader.NextResultAsync(cancellationToken);

        return result;
    }

    /// <inheritdoc/>
    public async Task<T> ReadSingleRequiredAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        CancellationToken cancellationToken = default) where T : class
    {
        var result = await ReadSingleAsync<T>(cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException(
                $"Expected at least one result of type {typeof(T).Name}, but found none.");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<List<T>> ReadListAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        CancellationToken cancellationToken = default) where T : class
    {
        ThrowIfDisposed();

        var results = new List<T>();

        if (!_hasMoreResults)
            return results;

        while (await _reader.ReadAsync(cancellationToken))
        {
            results.Add(_entityMapper.MapFromReader<T>(_reader));
        }

        // Bir sonraki result set'e geç
        _hasMoreResults = await _reader.NextResultAsync(cancellationToken);

        return results;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<T> ReadAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : class
    {
        ThrowIfDisposed();

        if (!_hasMoreResults)
            yield break;

        while (await _reader.ReadAsync(cancellationToken))
        {
            yield return _entityMapper.MapFromReader<T>(_reader);
        }

        // Bir sonraki result set'e geç
        _hasMoreResults = await _reader.NextResultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> NextResultAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!_hasMoreResults)
            return false;

        _hasMoreResults = await _reader.NextResultAsync(cancellationToken);
        return _hasMoreResults;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        // Output parametrelerini oku (reader kapanmadan önce)
        EnsureOutputParametersRead();

        await _reader.DisposeAsync();
        await _command.DisposeAsync();
        await _connection.DisposeAsync();

        _disposed = true;
    }

    private void EnsureOutputParametersRead()
    {
        if (_outputParameters != null)
            return;

        // Reader'ı kapat (output parametreleri okumak için gerekli)
        if (!_reader.IsClosed)
        {
            // Kalan tüm result setleri atla
            while (_reader.NextResult()) { }
        }

        if (_parameters != null)
        {
            _outputParameters = _parameters.GetOutputValues(_command);

            // Return value'yu ayıkla
            if (_outputParameters.TryGetValue("@ReturnValue", out var rv) && rv != null)
            {
                _returnValue = Convert.ToInt32(rv);
            }
        }
        else
        {
            _outputParameters = new Dictionary<string, object?>();
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
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

        // Common conversions
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
