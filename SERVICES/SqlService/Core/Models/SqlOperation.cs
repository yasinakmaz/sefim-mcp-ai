namespace SqlService.Core.Models;

/// <summary>
/// SQL operasyon tipleri
/// AOT-Compatible: Enum, reflection gerektirmez
/// </summary>
public enum SqlOperation
{
    /// <summary>
    /// SELECT sorgusu
    /// </summary>
    Select = 0,

    /// <summary>
    /// INSERT işlemi
    /// </summary>
    Insert = 1,

    /// <summary>
    /// UPDATE işlemi
    /// </summary>
    Update = 2,

    /// <summary>
    /// DELETE işlemi
    /// </summary>
    Delete = 3,

    /// <summary>
    /// Batch INSERT işlemi
    /// </summary>
    BatchInsert = 4,

    /// <summary>
    /// Batch UPDATE işlemi
    /// </summary>
    BatchUpdate = 5,

    /// <summary>
    /// Batch DELETE işlemi
    /// </summary>
    BatchDelete = 6,

    /// <summary>
    /// COUNT sorgusu
    /// </summary>
    Count = 7,

    /// <summary>
    /// Raw SQL sorgusu
    /// </summary>
    RawSql = 8,

    /// <summary>
    /// Transaction işlemi
    /// </summary>
    Transaction = 9
}
