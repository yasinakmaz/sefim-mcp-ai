using Microsoft.Data.SqlClient;

namespace SqlService.Core.Interfaces;

/// <summary>
/// SQL Connection factory interface - Bağlantı yönetimi
/// AOT-Compatible: No reflection
/// </summary>
public interface IConnectionFactory
{
    /// <summary>
    /// Yeni bir SqlConnection oluşturur ve açar
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Açık SqlConnection</returns>
    Task<SqlConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Connection string'i döndürür
    /// </summary>
    string GetConnectionString();

    /// <summary>
    /// Command timeout değerini döndürür
    /// </summary>
    int GetCommandTimeout();
}
