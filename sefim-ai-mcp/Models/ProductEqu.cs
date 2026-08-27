namespace SefimMcp.Models;

public class ProductEqu
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? ProductName { get; set; }

    public string? EquProductName { get; set; }

    public decimal? Multiplier { get; set; }

    public int? Choice1Id { get; set; }

    public int? Choice2Id { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
