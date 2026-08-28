namespace SefimMcp.Models;

public sealed class SalesReportItem : BillWithHeader
{
    public string? ProductCode { get; set; }

    public string? StockCode { get; set; }

    public decimal? PriceWithoutVAT { get; set; }

    public decimal? VATAmount { get; set; }

    public decimal? NetTutar { get; set; }
}
