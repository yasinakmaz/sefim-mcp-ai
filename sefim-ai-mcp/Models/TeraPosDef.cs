namespace SefimMcp.Models;

public class TeraPosDef
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string IpAddress { get; set; } = null!;

    public int Port { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
