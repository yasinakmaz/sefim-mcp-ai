namespace SqlService.Core.Attributes;

/// <summary>
/// Tablo adı ve şema bilgisini belirtir. Entity sınıflarına uygulanır.
/// AOT-Compatible: Compile-time metadata için kullanılır.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TableAttribute : Attribute
{
    /// <summary>
    /// Veritabanındaki tablo adı
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Veritabanı şeması (varsayılan: "dbo")
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// TableAttribute constructor
    /// </summary>
    /// <param name="name">Tablo adı</param>
    public TableAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>
    /// Tam tablo adını döndürür (Schema + Name)
    /// </summary>
    public string FullName => $"[{Schema}].[{Name}]";
}
