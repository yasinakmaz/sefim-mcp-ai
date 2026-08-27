namespace SefimMcp.Models;

public class DebitPayment
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? CustomerName { get; set; }

    public DateTime Date { get; set; }

    public decimal? CashPayment { get; set; }

    public decimal? CreditPayment { get; set; }

    public decimal? TicketPayment { get; set; }

    public string ReceivedByUserName { get; set; } = null!;

    public decimal? Discount { get; set; }

    public string? Description { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
