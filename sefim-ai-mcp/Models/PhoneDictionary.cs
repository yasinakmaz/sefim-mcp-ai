namespace SefimMcp.Models;

public class PhoneDictionary
{
    public string PhoneNumber { get; set; } = null!;

    public string? Address { get; set; }

    public string? CustomerName { get; set; }

    public string? OrderOwnerName { get; set; }

    public string? LastPaymentNote { get; set; }

    public string? Directions { get; set; }

    public string? LastCustomerNote { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }
}
