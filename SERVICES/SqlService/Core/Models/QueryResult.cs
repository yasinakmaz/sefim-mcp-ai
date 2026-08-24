namespace SqlService.Core.Models;

/// <summary>
/// Sorgu sonuç modeli - pagination desteği ile
/// AOT-Compatible: Generic constraint ile type-safe
/// </summary>
/// <typeparam name="T">Entity tipi</typeparam>
public sealed class QueryResult<T> where T : class
{
    /// <summary>
    /// Sorgu sonucu dönen kayıtlar
    /// </summary>
    public required IEnumerable<T> Data { get; init; }

    /// <summary>
    /// Toplam kayıt sayısı (pagination için)
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Sorgu süresi (milisaniye)
    /// </summary>
    public long ExecutionTimeMs { get; init; }

    /// <summary>
    /// Sayfa numarası (pagination için)
    /// </summary>
    public int? PageNumber { get; init; }

    /// <summary>
    /// Sayfa boyutu (pagination için)
    /// </summary>
    public int? PageSize { get; init; }

    /// <summary>
    /// Toplam sayfa sayısı (pagination için)
    /// </summary>
    public int? TotalPages => PageSize.HasValue && PageSize.Value > 0
        ? (int)Math.Ceiling(TotalCount / (double)PageSize.Value)
        : null;

    /// <summary>
    /// Sonraki sayfa var mı? (pagination için)
    /// </summary>
    public bool HasNextPage => PageNumber.HasValue && TotalPages.HasValue
        ? PageNumber.Value < TotalPages.Value
        : false;

    /// <summary>
    /// Önceki sayfa var mı? (pagination için)
    /// </summary>
    public bool HasPreviousPage => PageNumber.HasValue
        ? PageNumber.Value > 1
        : false;
}
