namespace SefimMcp.Models;

public class PaymentDetail
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public int PaymentId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentDetail1 { get; set; } = null!;

    public decimal Amount { get; set; }

    public string ReceivedByUserName { get; set; } = null!;

    public DateTime PaymentTime { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public string? KasaAciklama { get; set; }
}
