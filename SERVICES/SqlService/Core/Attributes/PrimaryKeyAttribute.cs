namespace SqlService.Core.Attributes;

/// <summary>
/// Primary key kolonu belirtir. Entity property'lerine uygulanır.
/// AOT-Compatible: Compile-time metadata için kullanılır.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class PrimaryKeyAttribute : Attribute
{
    /// <summary>
    /// Kolonun IDENTITY (auto-increment) olup olmadığı (varsayılan: true)
    /// </summary>
    public bool IsIdentity { get; set; } = true;

    /// <summary>
    /// Composite primary key için sıralama (0-based index)
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// PrimaryKeyAttribute constructor
    /// </summary>
    public PrimaryKeyAttribute()
    {
    }
}
