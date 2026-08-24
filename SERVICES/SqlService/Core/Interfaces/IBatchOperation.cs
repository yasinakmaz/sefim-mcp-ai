using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace SqlService.Core.Interfaces;

/// <summary>
/// Batch operations interface - Toplu işlemler
/// AOT-Compatible: No reflection
/// </summary>
public interface IBatchOperation
{
    /// <summary>
    /// Batch insert işlemi gerçekleştirir (SqlBulkCopy)
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="connection">SQL bağlantısı</param>
    /// <param name="entities">Eklenecek entity'ler</param>
    /// <param name="batchSize">Batch boyutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> BatchInsertAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        SqlConnection connection,
        IEnumerable<T> entities,
        int batchSize,
        CancellationToken cancellationToken) where T : class;

    /// <summary>
    /// Batch update işlemi gerçekleştirir (MERGE)
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="connection">SQL bağlantısı</param>
    /// <param name="entities">Güncellenecek entity'ler</param>
    /// <param name="batchSize">Batch boyutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> BatchUpdateAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        SqlConnection connection,
        IEnumerable<T> entities,
        int batchSize,
        CancellationToken cancellationToken) where T : class;

    /// <summary>
    /// Batch delete işlemi gerçekleştirir (IN clause)
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="connection">SQL bağlantısı</param>
    /// <param name="ids">Silinecek ID'ler</param>
    /// <param name="batchSize">Batch boyutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> BatchDeleteAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        SqlConnection connection,
        IEnumerable<object> ids,
        int batchSize,
        CancellationToken cancellationToken) where T : class;
}
