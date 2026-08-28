namespace SefimMcp.Models;

public sealed class EndOfTheDayReport
{
    public List<EndOfTheDayReceivable> Receivables { get; set; } = [];

    public List<DeletedBill> DeletedBills { get; set; } = [];

    public EndOfTheDaySummary Summary { get; set; } = new();

    public List<EndOfTheDayUserSale> UserSales { get; set; } = [];

    public List<EndOfTheDayProductSale> ProductSales { get; set; } = [];

    public List<PaidBill> FreeItems { get; set; } = [];

    public List<Payment> DebitPayments { get; set; } = [];

    public List<Payment> DiscountedPayments { get; set; } = [];

    public List<DirectTransaction> DirectTransactions { get; set; } = [];

    public List<EndOfTheDayReturnItem> ReturnItems { get; set; } = [];

    public List<PaidBill> WasteItems { get; set; } = [];

    public EndOfTheDayPrePaymentSummary PrePaymentSummary { get; set; } = new();
}

public sealed class EndOfTheDayReceivable
{
    public string? Source { get; set; }

    public string? TableNo { get; set; }

    public decimal? Debit { get; set; }

    public DateTime? PaymentTime { get; set; }

    public string? ReceivedByUserName { get; set; }

    public int? Key { get; set; }

    public string? CustomerName { get; set; }
}

public sealed class EndOfTheDaySummary
{
    public int TotalPaymentCount { get; set; }

    public decimal? CashPayment { get; set; }

    public decimal? CreditPayment { get; set; }

    public decimal? TicketPayment { get; set; }

    public decimal? OnlinePayment { get; set; }

    public decimal? Discount { get; set; }

    public decimal? Debit { get; set; }

    public decimal? ServiceTotal { get; set; }

    public decimal? Free { get; set; }

    public decimal? Zayi { get; set; }

    public decimal? ReturnSum { get; set; }

    public decimal? Canceled { get; set; }

    public decimal? OpenOrders { get; set; }

    public decimal? PakReturnsUM { get; set; }

    public decimal? DebitPayment_CashPayment { get; set; }

    public decimal? DebitPayment_CreditPayment { get; set; }

    public decimal? DebitPayment_TicketPayment { get; set; }

    public decimal? DebitPayment_OnlinePayment { get; set; }

    public decimal? Collect_CashPayment { get; set; }

    public decimal? Collect_CreditPayment { get; set; }

    public decimal? Collect_TicketPayment { get; set; }

    public decimal? Collect_OnlinePayment { get; set; }

    public decimal? Collect_Discount { get; set; }

    public decimal? DirectTransaction_CashPayment { get; set; }

    public decimal? PhoneOrderDebit { get; set; }

    public decimal? PhoneOrder_Discounts { get; set; }
}

public sealed class EndOfTheDayUserSale
{
    public decimal? Total { get; set; }

    public string? UserName { get; set; }
}

public sealed class EndOfTheDayProductSale
{
    public decimal? Quantity { get; set; }

    public decimal? Total { get; set; }

    public string? ProductName { get; set; }
}

public sealed class EndOfTheDayReturnItem
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string? UserName { get; set; }

    public string? ProductName { get; set; }

    public int? ProductId { get; set; }

    public int? Choice1Id { get; set; }

    public int? Choice2Id { get; set; }

    public string? Options { get; set; }

    public decimal? Price { get; set; }

    public decimal? Quantity { get; set; }

    public string? Comment { get; set; }

    public int HeaderId { get; set; }

    public int OrderId { get; set; }
}

public sealed class EndOfTheDayPrePaymentSummary
{
    public decimal? CashPayments { get; set; }

    public decimal? CreditPayments { get; set; }

    public decimal? TicketPayments { get; set; }

    public decimal? OnlinePayments { get; set; }

    public decimal? Discounts { get; set; }
}
