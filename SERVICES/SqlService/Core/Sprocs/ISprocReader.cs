using System.Diagnostics.CodeAnalysis;

namespace SqlService.Core.Sprocs;

/// <summary>
/// Stored Procedure multiple result set reader interface
/// AOT-Compatible: Generic constraint ile type-safe, zero reflection
/// </summary>
/// <remarks>
/// Dapper'ın GridReader'ına benzer, fakat:
/// - AOT/Trimming tam uyumlu
/// - Daha kolay kullanım (fluent API)
/// - Output parameter desteği dahil
/// </remarks>
/// <example>
/// // Tek result set
/// var users = await sprocService.QueryAsync&lt;UserDto&gt;("sp_GetUsers", params);
///
/// // Multiple result sets (2 adet)
/// var (totalCount, users) = await sprocService.QueryMultipleAsync&lt;int, List&lt;UserDto&gt;&gt;(
///     "sp_UserList",
///     new SprocParams().Add("@PageNumber", 1).Add("@PageSize", 20)
/// );
///
/// // Multiple result sets (manual reader)
/// await using var reader = await sprocService.QueryMultipleAsync("sp_ComplexProc", params);
/// var counts = await reader.ReadAsync&lt;int&gt;();
/// var users = await reader.ReadListAsync&lt;UserDto&gt;();
/// var roles = await reader.ReadListAsync&lt;RoleDto&gt;();
/// </example>
public interface ISprocReader : IAsyncDisposable
{
    /// <summary>
    /// Mevcut result set'ten tek bir değer okur (scalar)
    /// </summary>
    /// <typeparam name="T">Dönüş tipi (primitive: int, string, Guid, vb.)</typeparam>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Okunan değer veya default</returns>
    Task<T?> ReadScalarAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut result set'ten tek bir kayıt okur
    /// Sonra otomatik olarak bir sonraki result set'e geçer
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Okunan entity veya null</returns>
    Task<T?> ReadSingleAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Mevcut result set'ten tek bir kayıt okur (required - yoksa exception)
    /// Sonra otomatik olarak bir sonraki result set'e geçer
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Okunan entity</returns>
    /// <exception cref="InvalidOperationException">Kayıt bulunamadığında</exception>
    Task<T> ReadSingleRequiredAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Mevcut result set'ten tüm kayıtları liste olarak okur
    /// Sonra otomatik olarak bir sonraki result set'e geçer
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Okunan entity listesi</returns>
    Task<List<T>> ReadListAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Mevcut result set'ten tüm kayıtları IEnumerable olarak okur (lazy)
    /// Sonra otomatik olarak bir sonraki result set'e geçer
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Okunan entity'ler (IAsyncEnumerable)</returns>
    IAsyncEnumerable<T> ReadAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Bir sonraki result set'e manuel geçiş yapar
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Bir sonraki result set varsa true</returns>
    Task<bool> NextResultAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Daha fazla result set var mı?
    /// </summary>
    bool HasMoreResults { get; }

    /// <summary>
    /// Output parametrelerinin değerlerini döndürür
    /// </summary>
    IReadOnlyDictionary<string, object?> OutputParameters { get; }

    /// <summary>
    /// Return value'yu döndürür (varsa)
    /// </summary>
    int? ReturnValue { get; }
}
