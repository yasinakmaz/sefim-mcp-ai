namespace SefimMcp.Models;

public class ReasonList
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? Key { get; set; }

    public string? Description { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
