using System.Diagnostics.CodeAnalysis;

namespace SqlService.Core.Sprocs;

/// <summary>
/// Stored Procedure service interface - AOT-Compatible
/// Dapper'dan daha kolay kullanım, zero reflection
/// </summary>
/// <remarks>
/// Kullanım örnekleri:
///
/// 1. Tek result set (liste):
/// <code>
/// var users = await sprocService.QueryListAsync&lt;UserDto&gt;("sp_GetUsers",
///     new SprocParams().Add("@IsActive", true));
/// </code>
///
/// 2. Tek result set (tek kayıt):
/// <code>
/// var user = await sprocService.QuerySingleAsync&lt;UserDto&gt;("sp_GetUserById",
///     new SprocParams().Add("@UserId", userId));
/// </code>
///
/// 3. İki result set (count + data):
/// <code>
/// var (totalCount, users) = await sprocService.QueryAsync&lt;int, List&lt;UserDto&gt;&gt;(
///     "sp_UserList",
///     new SprocParams()
///         .Add("@PageNumber", 1)
///         .Add("@PageSize", 20));
/// </code>
///
/// 4. Üç result set:
/// <code>
/// var (users, roles, permissions) = await sprocService.QueryAsync&lt;List&lt;UserDto&gt;, List&lt;RoleDto&gt;, List&lt;PermissionDto&gt;&gt;(
///     "sp_GetUserWithRolesAndPermissions",
///     new SprocParams().Add("@UserId", userId));
/// </code>
///
/// 5. Manuel reader (4+ result set):
/// <code>
/// await using var reader = await sprocService.QueryMultipleAsync("sp_ComplexProc", params);
/// var first = await reader.ReadScalarAsync&lt;int&gt;();
/// var second = await reader.ReadListAsync&lt;UserDto&gt;();
/// var third = await reader.ReadListAsync&lt;RoleDto&gt;();
/// var fourth = await reader.ReadSingleAsync&lt;SettingsDto&gt;();
/// </code>
///
/// 6. Non-query (INSERT, UPDATE, DELETE):
/// <code>
/// var affected = await sprocService.ExecuteAsync("sp_UpdateUserStatus",
///     new SprocParams()
///         .Add("@UserId", userId)
///         .Add("@IsActive", false));
/// </code>
/// </remarks>
public interface ISprocService
{
    // ================== SINGLE RESULT SET ==================

    /// <summary>
    /// Tek result set dönen sproc çağırır - Liste olarak
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="sprocName">Stored procedure adı (schema.name veya sadece name)</param>
    /// <param name="parameters">Parametreler (null olabilir)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity listesi</returns>
    Task<List<T>> QueryListAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Tek result set dönen sproc çağırır - Tek kayıt (null olabilir)
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="sprocName">Stored procedure adı</param>
    /// <param name="parameters">Parametreler (null olabilir)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity veya null</returns>
    Task<T?> QuerySingleAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Tek result set dönen sproc çağırır - Tek kayıt (required)
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="sprocName">Stored procedure adı</param>
    /// <param name="parameters">Parametreler (null olabilir)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity</returns>
    /// <exception cref="InvalidOperationException">Kayıt bulunamadığında</exception>
    Task<T> QuerySingleRequiredAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Scalar değer dönen sproc çağırır (COUNT, SUM, vb.)
    /// </summary>
    /// <typeparam name="T">Dönüş tipi (int, string, Guid, vb.)</typeparam>
    /// <param name="sprocName">Stored procedure adı</param>
    /// <param name="parameters">Parametreler (null olabilir)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Scalar değer</returns>
    Task<T?> QueryScalarAsync<T>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default);

    // ================== TWO RESULT SETS ==================

    /// <summary>
    /// İki result set dönen sproc çağırır
    /// Örnek: sp_UserList → (TotalCount, Users)
    /// </summary>
    /// <typeparam name="T1">İlk result set tipi (scalar için int, string vb. / entity için UserDto vb.)</typeparam>
    /// <typeparam name="T2">İkinci result set tipi</typeparam>
    /// <param name="sprocName">Stored procedure adı</param>
    /// <param name="parameters">Parametreler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Tuple (T1, T2)</returns>
    Task<(T1? First, T2? Second)> QueryAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T1,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T2>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default)
        where T1 : class
        where T2 : class;

    /// <summary>
    /// İki result set dönen sproc çağırır - İlk sonuç scalar
    /// sp_UserList pattern: (int TotalCount, List&lt;UserDto&gt; Users)
    /// </summary>
    Task<(TScalar? First, List<TEntity> Second)> QueryScalarAndListAsync<TScalar,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] TEntity>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default) where TEntity : class;

    // ================== THREE RESULT SETS ==================

    /// <summary>
    /// Üç result set dönen sproc çağırır
    /// </summary>
    Task<(T1? First, T2? Second, T3? Third)> QueryAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T1,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T2,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T3>(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default)
        where T1 : class
        where T2 : class
        where T3 : class;

    // ================== FOUR RESULT SETS ==================

    /// <summary>
    /// Dört result set dönen sproc çağırır
    /// </summary>
    Task<(T1? First, T2? Second, T3? Third, T4? Fourth)> QueryAsync<
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
        where T4 : class;

    // ================== MANUAL READER (5+ RESULT SETS) ==================

    /// <summary>
    /// Multiple result set dönen sproc çağırır - Manuel reader ile
    /// 4'ten fazla result set için veya karmaşık senaryolar için
    /// </summary>
    /// <param name="sprocName">Stored procedure adı</param>
    /// <param name="parameters">Parametreler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>ISprocReader (IAsyncDisposable)</returns>
    Task<ISprocReader> QueryMultipleAsync(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default);

    // ================== NON-QUERY ==================

    /// <summary>
    /// Non-query sproc çağırır (INSERT, UPDATE, DELETE)
    /// </summary>
    /// <param name="sprocName">Stored procedure adı</param>
    /// <param name="parameters">Parametreler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> ExecuteAsync(
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Non-query sproc çağırır ve output parametrelerini döner
    /// </summary>
    /// <param name="sprocName">Stored procedure adı</param>
    /// <param name="parameters">Parametreler (output dahil)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Tuple (AffectedRows, OutputParameters)</returns>
    Task<(int AffectedRows, IReadOnlyDictionary<string, object?> Outputs)> ExecuteWithOutputAsync(
        string sprocName,
        SprocParams parameters,
        CancellationToken cancellationToken = default);
}
