namespace SefimMcp.Models;

public class Shift
{
    public DateTime Starts { get; set; }

    public DateTime? Ends { get; set; }

    public bool Aktarildi { get; set; }

    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
