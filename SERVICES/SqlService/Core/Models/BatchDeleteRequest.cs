namespace SqlService.Core.Models;

/// <summary>
/// Batch delete işlemi için istek modeli
/// AOT-Compatible: No reflection
/// </summary>
public sealed class BatchDeleteRequest
{
    /// <summary>
    /// Silinecek kayıtların ID'leri
    /// </summary>
    public required IEnumerable<object> Ids { get; init; }

    /// <summary>
    /// Bir batch'te kaç kayıt işlenecek - Varsayılan: 2000
    /// </summary>
    public int BatchSize { get; init; } = 2000;

    /// <summary>
    /// Primary key kolon adı - Varsayılan: "Id"
    /// </summary>
    public string ColumnName { get; init; } = "Id";

    /// <summary>
    /// Timeout süresi (saniye) - null ise default ayar kullanılır
    /// </summary>
    public int? TimeoutSeconds { get; init; }
}
