namespace SefimMcp.Models;

public class OrderHeader
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public DateTime? Date { get; set; }

    public string UserName { get; set; } = null!;

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
