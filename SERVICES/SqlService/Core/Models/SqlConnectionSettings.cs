namespace SqlService.Core.Models;

/// <summary>
/// SQL Server bağlantı ve performans ayarları
/// AOT-Compatible: Sadece property'ler, reflection yok
/// </summary>
public sealed class SqlConnectionSettings
{
    /// <summary>
    /// SQL Server bağlantı dizesi (gerekli)
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// SQL komut timeout süresi (saniye) - Varsayılan: 300
    /// </summary>
    public int CommandTimeout { get; set; } = 300;

    /// <summary>
    /// Batch insert işleminde bir seferde kaç kayıt işlenecek - Varsayılan: 1000
    /// </summary>
    public int BatchInsertSize { get; set; } = 1000;

    /// <summary>
    /// Batch update işleminde bir seferde kaç kayıt işlenecek - Varsayılan: 500
    /// </summary>
    public int BatchUpdateSize { get; set; } = 500;

    /// <summary>
    /// Batch delete işleminde bir seferde kaç kayıt işlenecek - Varsayılan: 2000
    /// </summary>
    public int BatchDeleteSize { get; set; } = 2000;

    /// <summary>
    /// Connection pooling etkinleştirme - Varsayılan: true
    /// </summary>
    public bool EnableConnectionPooling { get; set; } = true;

    /// <summary>
    /// Connection pool'da maksimum bağlantı sayısı - Varsayılan: 100
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Connection pool'da minimum bağlantı sayısı - Varsayılan: 5
    /// </summary>
    public int MinPoolSize { get; set; } = 5;

    /// <summary>
    /// Logging etkinleştirme - Varsayılan: true
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Ayarların geçerliliğini kontrol eder
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("ConnectionString is required.");

        if (CommandTimeout <= 0)
            throw new InvalidOperationException("CommandTimeout must be greater than 0.");

        if (BatchInsertSize <= 0)
            throw new InvalidOperationException("BatchInsertSize must be greater than 0.");

        if (BatchUpdateSize <= 0)
            throw new InvalidOperationException("BatchUpdateSize must be greater than 0.");

        if (BatchDeleteSize <= 0)
            throw new InvalidOperationException("BatchDeleteSize must be greater than 0.");

        if (MaxPoolSize <= 0)
            throw new InvalidOperationException("MaxPoolSize must be greater than 0.");

        if (MinPoolSize < 0)
            throw new InvalidOperationException("MinPoolSize cannot be negative.");

        if (MinPoolSize > MaxPoolSize)
            throw new InvalidOperationException("MinPoolSize cannot be greater than MaxPoolSize.");
    }
}
