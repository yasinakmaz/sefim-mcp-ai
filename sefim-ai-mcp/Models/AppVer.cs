namespace SefimMcp.Models;

public class AppVer
{
    public string Name { get; set; } = null!;

    public string? Value { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }
}
