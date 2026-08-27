namespace SefimMcp.Models;

public class TableLock
{
    public string TableNumber { get; set; } = null!;

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? UserName { get; set; }

    public bool? Locked { get; set; }
}
