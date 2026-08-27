namespace SefimMcp.Models;

public class Waste
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public string UserName { get; set; } = null!;

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
