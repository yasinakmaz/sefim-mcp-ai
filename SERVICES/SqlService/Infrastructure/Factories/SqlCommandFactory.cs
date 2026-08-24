using Microsoft.Data.SqlClient;
using SqlService.Core.Interfaces;

namespace SqlService.Infrastructure.Factories;

/// <summary>
/// SQL Command factory implementation
/// AOT-Compatible: No reflection
/// </summary>
public sealed class SqlCommandFactory
{
    private readonly IConnectionFactory _connectionFactory;

    public SqlCommandFactory(IConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Yeni bir SqlCommand oluşturur
    /// </summary>
    /// <param name="commandText">SQL komutu</param>
    /// <param name="connection">SQL bağlantısı (opsiyonel)</param>
    /// <returns>SqlCommand</returns>
    public SqlCommand CreateCommand(string commandText, SqlConnection? connection = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandText);

        var command = connection != null
            ? new SqlCommand(commandText, connection)
            : new SqlCommand(commandText);

        command.CommandTimeout = _connectionFactory.GetCommandTimeout();

        return command;
    }

    /// <summary>
    /// SqlCommand'a parametre ekler (SQL injection safe)
    /// </summary>
    /// <param name="command">SqlCommand</param>
    /// <param name="name">Parametre adı (@ile başlamalı)</param>
    /// <param name="value">Parametre değeri</param>
    public void AddParameter(SqlCommand command, string name, object? value)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // @ ile başlamıyorsa ekle
        if (!name.StartsWith('@'))
            name = $"@{name}";

        command.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    /// <summary>
    /// SqlCommand'a birden fazla parametre ekler
    /// </summary>
    /// <param name="command">SqlCommand</param>
    /// <param name="parameters">Parametreler (key-value pairs)</param>
    public void AddParameters(SqlCommand command, IReadOnlyDictionary<string, object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(parameters);

        foreach (var parameter in parameters)
        {
            AddParameter(command, parameter.Key, parameter.Value);
        }
    }
}
