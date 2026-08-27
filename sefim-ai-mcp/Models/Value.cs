namespace SefimMcp.Models;

public class Value
{
    public string Name { get; set; } = null!;

    public string? Value1 { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }
}
