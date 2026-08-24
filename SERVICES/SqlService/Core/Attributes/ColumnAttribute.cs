namespace SqlService.Core.Attributes;

/// <summary>
/// Kolon adı ve metadata bilgisini belirtir. Entity property'lerine uygulanır.
/// AOT-Compatible: Reflection-free mapping için kullanılır.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class ColumnAttribute : Attribute
{
    /// <summary>
    /// Veritabanındaki kolon adı
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Kolonun null değer alıp alamayacağı (varsayılan: true)
    /// </summary>
    public bool IsNullable { get; set; } = true;

    /// <summary>
    /// String kolonlar için maksimum uzunluk
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// SQL Server veri tipi (örn: SqlDbType.NVarChar)
    /// </summary>
    public Type? DbType { get; set; }

    /// <summary>
    /// ColumnAttribute constructor
    /// </summary>
    /// <param name="name">Kolon adı</param>
    public ColumnAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}
