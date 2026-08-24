namespace SqlService.Core.Models;

/// <summary>
/// Batch insert işlemi için istek modeli
/// AOT-Compatible: Generic constraint ile type-safe
/// </summary>
/// <typeparam name="T">Entity tipi</typeparam>
public sealed class BatchInsertRequest<T> where T : class
{
    /// <summary>
    /// Insert edilecek entity'ler
    /// </summary>
    public required IEnumerable<T> Entities { get; init; }

    /// <summary>
    /// Bir batch'te kaç kayıt işlenecek - Varsayılan: 1000
    /// </summary>
    public int BatchSize { get; init; } = 1000;

    /// <summary>
    /// IDENTITY kolonlarını yok say (INSERT INTO ile IDENTITY_INSERT ON gerektirir)
    /// </summary>
    public bool IgnoreIdentity { get; init; } = false;

    /// <summary>
    /// Timeout süresi (saniye) - null ise default ayar kullanılır
    /// </summary>
    public int? TimeoutSeconds { get; init; }
}
