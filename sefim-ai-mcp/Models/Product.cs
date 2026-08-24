namespace SefimMcp.Models;

public class Product
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? ProductName { get; set; }

    public string? ProductGroup { get; set; }

    public string? ProductCode { get; set; }

    public string? Order { get; set; }

    public decimal? Price { get; set; }

    public decimal? VatRate { get; set; }

    public bool? FreeItem { get; set; }

    public string? InvoiceName { get; set; }

    public string? ProductType { get; set; }

    public string? Plu { get; set; }

    public bool? SkipOptionSelection { get; set; }

    public string? Favorites { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public string? IstisnaKodu { get; set; }

    public string? OzelMatrahKodu { get; set; }

    public string? StockCode { get; set; }

    public bool? HasBom { get; set; }

    public bool? HasOptions { get; set; }

    public bool? HasEqu { get; set; }
}
