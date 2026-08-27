namespace SefimMcp.Models;

public class Deliverer
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public string? Code { get; set; }
}
