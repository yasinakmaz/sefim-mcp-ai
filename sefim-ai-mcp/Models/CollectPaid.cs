namespace SefimMcp.Models;

public class CollectPaid
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? Deliverer { get; set; }

    public decimal? CashPayment { get; set; }

    public decimal? CreditPayment { get; set; }

    public decimal? TicketPayment { get; set; }

    public DateTime? PaymentTime { get; set; }

    public string? ReceivedByUserName { get; set; }

    public decimal? Discount { get; set; }

    public string SubType { get; set; } = null!;

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
