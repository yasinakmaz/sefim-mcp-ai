namespace SqlService.Core.Models;

/// <summary>
/// Batch update işlemi için istek modeli
/// AOT-Compatible: Generic constraint ile type-safe
/// </summary>
/// <typeparam name="T">Entity tipi</typeparam>
public sealed class BatchUpdateRequest<T> where T : class
{
    /// <summary>
    /// Update edilecek entity'ler
    /// </summary>
    public required IEnumerable<T> Entities { get; init; }

    /// <summary>
    /// Bir batch'te kaç kayıt işlenecek - Varsayılan: 500
    /// </summary>
    public int BatchSize { get; init; } = 500;

    /// <summary>
    /// Update edilecek kolonlar (null ise tüm non-PK kolonlar)
    /// </summary>
    public string[]? UpdateColumns { get; init; }

    /// <summary>
    /// Timeout süresi (saniye) - null ise default ayar kullanılır
    /// </summary>
    public int? TimeoutSeconds { get; init; }
}
