namespace SefimMcp.Models;

public class MenuProduct
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public int MenuId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal Price { get; set; }

    public bool? Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public int? ProductId { get; set; }
}
