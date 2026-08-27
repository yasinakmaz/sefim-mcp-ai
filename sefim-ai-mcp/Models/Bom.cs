namespace SefimMcp.Models;

public class Bom
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? ProductName { get; set; }

    public string? MaterialName { get; set; }

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = null!;

    public int StokId { get; set; }

    public int? ProductId { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
