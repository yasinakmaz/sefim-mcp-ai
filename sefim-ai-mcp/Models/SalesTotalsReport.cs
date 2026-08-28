namespace SefimMcp.Models;

public sealed class SalesTotalsReport
{
    public string? ProductName { get; set; }

    public decimal? Total { get; set; }

    public decimal? Quantity { get; set; }

    public string? ProductCode { get; set; }

    public string? StockCode { get; set; }

    public decimal? PriceWithoutVAT { get; set; }

    public decimal? VATAmount { get; set; }
}
