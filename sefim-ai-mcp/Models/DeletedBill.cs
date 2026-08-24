using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class DeletedBill
{
    public int Id { get; set; }

    public int BillState { get; set; }

    public int BillType { get; set; }

    public int? SubBillNo { get; set; }

    public int? PaymentId { get; set; }

    public DateTime Date { get; set; }

    public string UserName { get; set; } = null!;

    public string? TableNumber { get; set; }

    public string? ProductName { get; set; }

    public int? ProductId { get; set; }

    public int? Choice1Id { get; set; }

    public int? Choice2Id { get; set; }

    public string? Options { get; set; }

    public decimal? Price { get; set; }

    public decimal? Quantity { get; set; }

    public string? Comment { get; set; }

    public string DeletingUserName { get; set; } = null!;

    public DateTime DeletingTime { get; set; }

    public int HeaderId { get; set; }

    public int OrderId { get; set; }

    public decimal? OriginalPrice { get; set; }

    public int? BillId { get; set; }

    public string? DeleteReason { get; set; }

    public bool? Printed { get; set; }

    public int? OrderState { get; set; }

    public string? DeleteDetails { get; set; }

    public string Fisno { get; set; } = null!;

    public bool Integration { get; set; }

    public string? DailyBillNumber { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public int? DailyNumber { get; set; }

    public string? MenuName { get; set; }
}
