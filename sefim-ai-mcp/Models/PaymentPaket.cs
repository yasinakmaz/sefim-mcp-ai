using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class PaymentPaket
{
    public int Id { get; set; }

    public string? TableNo { get; set; }

    public decimal? CashPayment { get; set; }

    public decimal? CreditPayment { get; set; }

    public decimal? TicketPayment { get; set; }

    public decimal? OnlinePayment { get; set; }

    public decimal? Discount { get; set; }

    public decimal? Debit { get; set; }

    public string? CustomerName { get; set; }

    public DateTime? PaymentTime { get; set; }

    public string? ReceivedByUserName { get; set; }

    public int? HeaderId { get; set; }

    public string? DiscountReason { get; set; }

    public string? PersonName { get; set; }

    public string? InvoiceNo { get; set; }

    public string? IdentificationNo { get; set; }

    public string? KrediKarti { get; set; }

    public string? YemekKarti { get; set; }

    public string? Deliverer { get; set; }

    public bool? FastSale { get; set; }

    public string? OnlineOdeme { get; set; }

    public string? Description { get; set; }

    public bool? Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public bool? EfaturaTipi { get; set; }

    public string? Ettn { get; set; }

    public string? BelgeNotu { get; set; }

    public short? AktarimDurumu { get; set; }
}
