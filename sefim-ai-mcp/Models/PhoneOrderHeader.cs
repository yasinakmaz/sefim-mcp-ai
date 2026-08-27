namespace SefimMcp.Models;

public class PhoneOrderHeader
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? Address { get; set; }

    public string? CustomerName { get; set; }

    public string? OrderNo { get; set; }

    public string? OrderOwnerName { get; set; }

    public string? PaymentNote { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Deliverer { get; set; }

    public bool? Paid { get; set; }

    public int HeaderId { get; set; }

    public DateTime CreationTime { get; set; }

    public string CreatedByUserName { get; set; } = null!;

    public string? Directions { get; set; }

    public string? CustomerNote { get; set; }

    public decimal? Discount { get; set; }

    public string? TrackingNumber { get; set; }

    public DateTime? AssignDate { get; set; }

    public string? PaymentDetail { get; set; }

    public int DailyNumber { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public DateTime? ScheduledDate { get; set; }

    public int? PaketEntegrasyonId { get; set; }

    public int? PvEntId { get; set; }

    public bool? PvPaid { get; set; }

    public string? OrderStatus { get; set; }
}
