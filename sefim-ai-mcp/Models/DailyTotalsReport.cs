namespace SefimMcp.Models;

public sealed class DailyTotalsReport
{
    public DateTime Date { get; set; }

    public int TotalPaymentCount { get; set; }

    public decimal? CashPayment { get; set; }

    public decimal? CreditPayment { get; set; }

    public decimal? TicketPayment { get; set; }

    public decimal? OnlinePayment { get; set; }

    public decimal? Discount { get; set; }

    public decimal? Debit { get; set; }

    public decimal? Free { get; set; }

    public decimal? ResReturnItems { get; set; }

    public decimal? Canceled { get; set; }

    public decimal? DebitPayment_CashPayment { get; set; }

    public decimal? DebitPayment_CreditPayment { get; set; }

    public decimal? DebitPayment_Discount { get; set; }

    public decimal? DebitPayment_TicketPayment { get; set; }

    public decimal? Collect_CashPayment { get; set; }

    public decimal? Collect_CreditPayment { get; set; }

    public decimal? Collect_Discount { get; set; }

    public decimal? Collect_TicketPayment { get; set; }

    public decimal? Collect_OnlinePayment { get; set; }

    public decimal? PakReturnItems { get; set; }

    public decimal? DirectTransaction_CashPayment { get; set; }
}
