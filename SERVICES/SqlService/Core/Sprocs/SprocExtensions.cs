using System.Diagnostics.CodeAnalysis;

namespace SqlService.Core.Sprocs;

/// <summary>
/// ISprocService extension methods - Daha kolay kullanım için
/// AOT-Compatible: Zero reflection
/// </summary>
public static class SprocExtensions
{
    /// <summary>
    /// Sayfalama sonucu dönen sproc çağırır
    /// Pattern: İlk result set = TotalCount (int), İkinci result set = Data (List&lt;T&gt;)
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="sprocService">ISprocService</param>
    /// <param name="sprocName">Stored procedure adı</param>
    /// <param name="parameters">Parametreler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>PagedResult (TotalCount + Data)</returns>
    /// <example>
    /// var result = await sprocService.QueryPagedAsync&lt;UserDto&gt;("sp_UserList",
    ///     new SprocParams()
    ///         .Add("@PageNumber", 1)
    ///         .Add("@PageSize", 20)
    ///         .AddNullable("@Search", searchText));
    ///
    /// Console.WriteLine($"Total: {result.TotalCount}, Items: {result.Data.Count}");
    /// </example>
    public static async Task<SprocPagedResult<T>> QueryPagedAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        this ISprocService sprocService,
        string sprocName,
        SprocParams? parameters = null,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(sprocService);

        var (totalCount, data) = await sprocService.QueryScalarAndListAsync<int?, T>(
            sprocName, parameters, cancellationToken);

        return new SprocPagedResult<T>
        {
            TotalCount = totalCount ?? 0,
            Data = data
        };
    }

    /// <summary>
    /// Sayfalama sonucu dönen sproc çağırır - PageNumber ve PageSize parametreli
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="sprocService">ISprocService</param>
    /// <param name="sprocName">Stored procedure adı</param>
    /// <param name="pageNumber">Sayfa numarası (1-based)</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <param name="additionalParams">Ek parametreler (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>PagedResult</returns>
    /// <example>
    /// var result = await sprocService.QueryPagedAsync&lt;UserDto&gt;("sp_UserList", 1, 20,
    ///     new SprocParams().AddNullable("@Search", "john"));
    /// </example>
    public static async Task<SprocPagedResult<T>> QueryPagedAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        this ISprocService sprocService,
        string sprocName,
        int pageNumber,
        int pageSize,
        SprocParams? additionalParams = null,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(sprocService);

        var parameters = additionalParams ?? new SprocParams();
        parameters.Add("@PageNumber", pageNumber);
        parameters.Add("@PageSize", pageSize);

        return await sprocService.QueryPagedAsync<T>(sprocName, parameters, cancellationToken);
    }
}

/// <summary>
/// Sayfalama sonucu - Sproc için
/// </summary>
/// <typeparam name="T">Entity tipi</typeparam>
public sealed class SprocPagedResult<T> where T : class
{
    /// <summary>
    /// Toplam kayıt sayısı
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Sayfa verileri
    /// </summary>
    public required List<T> Data { get; init; }

    /// <summary>
    /// Veri var mı?
    /// </summary>
    public bool HasData => Data.Count > 0;

    /// <summary>
    /// Boş sonuç
    /// </summary>
    public static SprocPagedResult<T> Empty => new()
    {
        TotalCount = 0,
        Data = []
    };
}
