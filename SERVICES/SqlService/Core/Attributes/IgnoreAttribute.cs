namespace SqlService.Core.Attributes;

/// <summary>
/// Bu property'nin veritabanı mapping'inde yok sayılacağını belirtir.
/// AOT-Compatible: Compile-time metadata için kullanılır.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class IgnoreAttribute : Attribute
{
    /// <summary>
    /// IgnoreAttribute constructor
    /// </summary>
    public IgnoreAttribute()
    {
    }
}
