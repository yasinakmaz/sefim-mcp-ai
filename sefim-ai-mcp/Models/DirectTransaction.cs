namespace SefimMcp.Models;

public class DirectTransaction
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string? Description { get; set; }

    public decimal Total { get; set; }

    public string UserName { get; set; } = null!;

    public string? CustomerName { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
