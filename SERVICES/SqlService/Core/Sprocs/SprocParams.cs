using Microsoft.Data.SqlClient;
using System.Data;

namespace SqlService.Core.Sprocs;

/// <summary>
/// Stored Procedure parametre builder - Fluent API
/// AOT-Compatible: Zero reflection, type-safe
/// </summary>
/// <example>
/// var parameters = new SprocParams()
///     .Add("@PageNumber", 1)
///     .Add("@PageSize", 20)
///     .Add("@Search", searchText)
///     .AddNullable("@RoleCode", roleCode)
///     .AddOutput("@TotalCount", SqlDbType.Int);
/// </example>
public sealed class SprocParams
{
    private readonly List<SprocParameter> _parameters = new();

    /// <summary>
    /// Parametre sayısı
    /// </summary>
    public int Count => _parameters.Count;

    /// <summary>
    /// Parametre ekler (required)
    /// </summary>
    /// <param name="name">Parametre adı (@ile başlamalı)</param>
    /// <param name="value">Parametre değeri</param>
    /// <returns>Fluent builder</returns>
    public SprocParams Add(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureAtPrefix(ref name);

        _parameters.Add(new SprocParameter
        {
            Name = name,
            Value = value ?? DBNull.Value,
            Direction = ParameterDirection.Input
        });

        return this;
    }

    /// <summary>
    /// Parametre ekler - tip belirtilerek (SqlDbType)
    /// </summary>
    public SprocParams Add(string name, object? value, SqlDbType dbType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureAtPrefix(ref name);

        _parameters.Add(new SprocParameter
        {
            Name = name,
            Value = value ?? DBNull.Value,
            DbType = dbType,
            Direction = ParameterDirection.Input
        });

        return this;
    }

    /// <summary>
    /// Parametre ekler - tip ve boyut belirtilerek
    /// </summary>
    public SprocParams Add(string name, object? value, SqlDbType dbType, int size)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureAtPrefix(ref name);

        _parameters.Add(new SprocParameter
        {
            Name = name,
            Value = value ?? DBNull.Value,
            DbType = dbType,
            Size = size,
            Direction = ParameterDirection.Input
        });

        return this;
    }

    /// <summary>
    /// Nullable parametre ekler - null ise DBNull.Value gönderir
    /// </summary>
    public SprocParams AddNullable<T>(string name, T? value) where T : struct
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureAtPrefix(ref name);

        _parameters.Add(new SprocParameter
        {
            Name = name,
            Value = value.HasValue ? (object)value.Value : DBNull.Value,
            Direction = ParameterDirection.Input
        });

        return this;
    }

    /// <summary>
    /// Nullable string parametre ekler
    /// </summary>
    public SprocParams AddNullable(string name, string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureAtPrefix(ref name);

        _parameters.Add(new SprocParameter
        {
            Name = name,
            Value = string.IsNullOrEmpty(value) ? DBNull.Value : value,
            Direction = ParameterDirection.Input
        });

        return this;
    }

    /// <summary>
    /// Output parametre ekler
    /// </summary>
    public SprocParams AddOutput(string name, SqlDbType dbType, int size = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureAtPrefix(ref name);

        _parameters.Add(new SprocParameter
        {
            Name = name,
            Value = DBNull.Value,
            DbType = dbType,
            Size = size,
            Direction = ParameterDirection.Output
        });

        return this;
    }

    /// <summary>
    /// InputOutput parametre ekler
    /// </summary>
    public SprocParams AddInputOutput(string name, object? value, SqlDbType dbType, int size = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureAtPrefix(ref name);

        _parameters.Add(new SprocParameter
        {
            Name = name,
            Value = value ?? DBNull.Value,
            DbType = dbType,
            Size = size,
            Direction = ParameterDirection.InputOutput
        });

        return this;
    }

    /// <summary>
    /// Return value parametresi ekler
    /// </summary>
    public SprocParams AddReturnValue(string name = "@ReturnValue")
    {
        EnsureAtPrefix(ref name);

        _parameters.Add(new SprocParameter
        {
            Name = name,
            Value = DBNull.Value,
            DbType = SqlDbType.Int,
            Direction = ParameterDirection.ReturnValue
        });

        return this;
    }

    /// <summary>
    /// SqlCommand'a parametreleri uygular
    /// </summary>
    internal void ApplyTo(SqlCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        foreach (var param in _parameters)
        {
            var sqlParam = new SqlParameter
            {
                ParameterName = param.Name,
                Value = param.Value,
                Direction = param.Direction
            };

            if (param.DbType.HasValue)
            {
                sqlParam.SqlDbType = param.DbType.Value;
            }

            if (param.Size > 0)
            {
                sqlParam.Size = param.Size;
            }

            command.Parameters.Add(sqlParam);
        }
    }

    /// <summary>
    /// Output/Return parametrelerinin değerlerini okur
    /// </summary>
    internal Dictionary<string, object?> GetOutputValues(SqlCommand command)
    {
        var outputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var param in _parameters.Where(p =>
            p.Direction == ParameterDirection.Output ||
            p.Direction == ParameterDirection.InputOutput ||
            p.Direction == ParameterDirection.ReturnValue))
        {
            var sqlParam = command.Parameters[param.Name];
            outputs[param.Name] = sqlParam.Value == DBNull.Value ? null : sqlParam.Value;
        }

        return outputs;
    }

    /// <summary>
    /// Parametrelerin listesini döndürür (readonly)
    /// </summary>
    internal IReadOnlyList<SprocParameter> GetParameters() => _parameters.AsReadOnly();

    private static void EnsureAtPrefix(ref string name)
    {
        if (!name.StartsWith('@'))
        {
            name = "@" + name;
        }
    }
}

/// <summary>
/// Sproc parametre bilgisi (internal)
/// </summary>
internal sealed class SprocParameter
{
    public required string Name { get; init; }
    public required object Value { get; init; }
    public SqlDbType? DbType { get; init; }
    public int Size { get; init; }
    public ParameterDirection Direction { get; init; }
}
