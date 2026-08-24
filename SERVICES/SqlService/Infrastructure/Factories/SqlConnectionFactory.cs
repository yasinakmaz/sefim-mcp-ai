using Microsoft.Data.SqlClient;
using SqlService.Core.Interfaces;
using SqlService.Core.Models;
using Microsoft.Extensions.Logging;

namespace SqlService.Infrastructure.Factories;

/// <summary>
/// SQL Connection factory implementation - Connection pooling destekli
/// AOT-Compatible: No reflection
/// </summary>
public sealed class SqlConnectionFactory : IConnectionFactory
{
    private readonly SqlConnectionSettings _settings;
    private readonly ILogger<SqlConnectionFactory>? _logger;
    private readonly string _connectionString;

    public SqlConnectionFactory(
        SqlConnectionSettings settings,
        ILogger<SqlConnectionFactory>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Validate();

        _settings = settings;
        _logger = logger;

        // Connection string builder ile optimize edilmiş connection string oluştur
        var builder = new SqlConnectionStringBuilder(_settings.ConnectionString)
        {
            Pooling = _settings.EnableConnectionPooling,
            MaxPoolSize = _settings.MaxPoolSize,
            MinPoolSize = _settings.MinPoolSize,
            ConnectTimeout = 30,
            TrustServerCertificate = true,
            MultipleActiveResultSets = true, // SQL Client 6.0+ MARS is AOT-safe
            Encrypt = true
        };

        _connectionString = builder.ConnectionString;

        _logger?.LogInformation(
            "SqlConnectionFactory initialized. Pooling={Pooling}, MaxPoolSize={MaxPoolSize}, MinPoolSize={MinPoolSize}",
            _settings.EnableConnectionPooling,
            _settings.MaxPoolSize,
            _settings.MinPoolSize);
    }

    /// <inheritdoc/>
    public async Task<SqlConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            _logger?.LogDebug(
                "SQL Connection opened. State={State}, Database={Database}",
                connection.State,
                connection.Database);

            return connection;
        }
        catch (SqlException ex)
        {
            _logger?.LogError(ex,
                "Failed to open SQL connection. Error={Message}",
                ex.Message);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("SQL connection open operation was cancelled.");
            throw;
        }
    }

    /// <inheritdoc/>
    public string GetConnectionString() => _connectionString;

    /// <inheritdoc/>
    public int GetCommandTimeout() => _settings.CommandTimeout;
}
