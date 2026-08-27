namespace SefimMcp.Models;

public class ProductLongName
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public int? ProductId { get; set; }

    public int? Choice1 { get; set; }

    public int? Choice2 { get; set; }

    public string? ProductName { get; set; }

    public bool? HasBom { get; set; }

    public bool? HasEqu { get; set; }

    public bool? HasOptions { get; set; }

    public bool? Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
