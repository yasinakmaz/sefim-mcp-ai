namespace SefimMcp.Models;

public class BillHeader
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public int BillState { get; set; }

    public int BillType { get; set; }

    public int? SubBillNo { get; set; }

    public int? PaymentId { get; set; }

    public string? TableNumber { get; set; }

    public int? Persons { get; set; }

    public string Fisno { get; set; } = null!;

    public string? FormalBillId { get; set; }

    public string? DailyBillNumber { get; set; }

    public string? TableGroupId { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public DateTime? BeginTime { get; set; }

    public DateTime? CloseTime { get; set; }

    public short? AktarimDurumu { get; set; }

    public string? PavoOrderNo { get; set; }
}
