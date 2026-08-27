namespace SefimMcp.Models;

public class DigiPan
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? Date { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
