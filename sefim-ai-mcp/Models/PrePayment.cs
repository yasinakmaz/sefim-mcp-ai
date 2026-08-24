using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class PrePayment
{
    public int Id { get; set; }

    public string? TableNumber { get; set; }

    public string? PaymentType { get; set; }

    public decimal? Total { get; set; }

    public string? Details { get; set; }

    public DateTime? PaymentTime { get; set; }

    public decimal? CashPayment { get; set; }

    public decimal? CreditPayment { get; set; }

    public decimal? TicketPayment { get; set; }

    public decimal? OnlinePayment { get; set; }

    public decimal? Discount { get; set; }

    public string? UserName { get; set; }

    public string? KrediKarti { get; set; }

    public string? YemekKarti { get; set; }

    public string? Online { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public bool? Pos { get; set; }

    public decimal? KdvtevkifatTutari { get; set; }
}
