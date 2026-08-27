namespace SefimMcp.Models;

public class Customer
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? CustomerName { get; set; }

    public string? TaxOffice { get; set; }

    public string? TaxNumber { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? Category { get; set; }

    public string? PhoneNumber { get; set; }

    public string? CustomerCode { get; set; }

    public string? FullName { get; set; }

    public decimal? DiscountRate { get; set; }

    public bool? Passive { get; set; }

    public decimal? CreditAllowance { get; set; }

    public string? CardNo { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public bool? EfaturaUser { get; set; }

    public string? Adi { get; set; }

    public string? Soyadi { get; set; }

    public string? Eposta { get; set; }

    public string? TicariSicilNo { get; set; }

    public bool? Kdvtevkifatli { get; set; }
}
