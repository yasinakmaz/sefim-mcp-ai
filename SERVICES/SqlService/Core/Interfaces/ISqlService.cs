using SqlService.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace SqlService.Core.Interfaces;

/// <summary>
/// Ana SQL Service Interface - Tüm veritabanı işlemlerine erişim
/// AOT-Compatible: Generic constraint ile type-safe, no reflection
/// </summary>
/// <typeparam name="T">Entity tipi (strongly-typed, AOT-compatible)</typeparam>
public interface ISqlService<[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.PublicProperties |
    DynamicallyAccessedMemberTypes.PublicConstructors)] T> where T : class
{
    // ================== QUERY OPERATIONS ==================

    /// <summary>
    /// Tek bir kaydı ID'ye göre sorgular
    /// </summary>
    /// <param name="id">Primary key değeri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Bulunan entity veya null</returns>
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tüm kayıtları sorgular
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Tüm entity'ler</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// WHERE koşulu ile sorgular (SQL injection safe)
    /// </summary>
    /// <param name="sqlWhere">WHERE koşulu (örn: "Age > @Age AND Name LIKE @Name")</param>
    /// <param name="parameters">Parametreler (key-value pairs)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Koşula uyan entity'ler</returns>
    Task<IEnumerable<T>> GetWhereAsync(
        string sqlWhere,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sayfalama ile kayıtları getirir (pagination)
    /// </summary>
    /// <param name="pageNumber">Sayfa numarası (1-based)</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfa verileri ve toplam kayıt sayısı</returns>
    Task<QueryResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Koşula göre kaç kayıt olduğunu sayar
    /// </summary>
    /// <param name="sqlWhere">WHERE koşulu (opsiyonel)</param>
    /// <param name="parameters">Parametreler (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kayıt sayısı</returns>
    Task<int> CountAsync(
        string? sqlWhere = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kaydın var olup olmadığını kontrol eder
    /// </summary>
    /// <param name="id">Primary key değeri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kayıt varsa true</returns>
    Task<bool> ExistsAsync(object id, CancellationToken cancellationToken = default);

    // ================== SINGLE INSERT/UPDATE/DELETE ==================

    /// <summary>
    /// Tek bir kayıt ekler (reflection-free)
    /// </summary>
    /// <param name="entity">Eklenecek entity</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> InsertAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tek bir kayıt ekler ve Identity değerini döndürür
    /// Entity'de [PrimaryKey(IsIdentity = true)] tanımlıysa OUTPUT INSERTED ile ID döner
    /// Identity yoksa null döner (ama kayıt eklenir)
    /// </summary>
    /// <typeparam name="TKey">Identity tipi (int, long, Guid, vb.)</typeparam>
    /// <param name="entity">Eklenecek entity</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Eklenen kaydın Identity değeri veya null (Identity yoksa)</returns>
    Task<TKey?> InsertAndGetIdAsync<TKey>(T entity, CancellationToken cancellationToken = default) where TKey : struct;

    /// <summary>
    /// Tek bir kaydı günceller
    /// </summary>
    /// <param name="entity">Güncellenecek entity</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tek bir kaydı siler
    /// </summary>
    /// <param name="id">Primary key değeri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> DeleteAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Koşula göre kayıtları siler
    /// </summary>
    /// <param name="sqlWhere">WHERE koşulu</param>
    /// <param name="parameters">Parametreler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> DeleteWhereAsync(
        string sqlWhere,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);

    // ================== BATCH OPERATIONS (Zero-Reflection) ==================

    /// <summary>
    /// Bir seferde birden fazla kayıt ekler (SqlBulkCopy based)
    /// </summary>
    /// <param name="entities">Eklenecek entity'ler</param>
    /// <param name="batchSize">Batch boyutu (varsayılan: 1000)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> BatchInsertAsync(
        IEnumerable<T> entities,
        int batchSize = 1000,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir seferde birden fazla kaydı günceller (MERGE statement)
    /// </summary>
    /// <param name="entities">Güncellenecek entity'ler</param>
    /// <param name="batchSize">Batch boyutu (varsayılan: 500)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> BatchUpdateAsync(
        IEnumerable<T> entities,
        int batchSize = 500,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir seferde birden fazla kaydı siler (IN clause)
    /// </summary>
    /// <param name="ids">Silinecek primary key değerleri</param>
    /// <param name="batchSize">Batch boyutu (varsayılan: 2000)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> BatchDeleteAsync(
        IEnumerable<object> ids,
        int batchSize = 2000,
        CancellationToken cancellationToken = default);

    // ================== TRANSACTION SUPPORT ==================

    /// <summary>
    /// Transaction başlatır
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Transaction nesnesi (IAsyncDisposable)</returns>
    Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default);

    // ================== ADVANCED ==================

    /// <summary>
    /// Raw SQL sorgusu çalıştırır (output mapping ile)
    /// </summary>
    /// <param name="sql">SQL sorgusu</param>
    /// <param name="parameters">Parametreler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sorgu sonucu entity'ler</returns>
    Task<IEnumerable<T>> ExecuteRawQueryAsync(
        string sql,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Raw SQL non-query komut çalıştırır (INSERT, UPDATE, DELETE, vb.)
    /// </summary>
    /// <param name="sql">SQL komutu</param>
    /// <param name="parameters">Parametreler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> ExecuteNonQueryAsync(
        string sql,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Raw SQL scalar sorgusu (COUNT, SUM, MAX, vb.)
    /// </summary>
    /// <param name="sql">SQL sorgusu</param>
    /// <param name="parameters">Parametreler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Scalar sonuç</returns>
    Task<TResult?> ExecuteScalarAsync<TResult>(
        string sql,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}
