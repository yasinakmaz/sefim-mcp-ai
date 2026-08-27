namespace SefimMcp.Models;

public class PrintJob
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public int? BillHeaderId { get; set; }

    public int? OrderHeaderId { get; set; }

    public int JobType { get; set; }

    public string UserName { get; set; } = null!;

    public int? BillId { get; set; }

    public decimal? Discount { get; set; }

    public string? ComputerName { get; set; }

    public string? Data { get; set; }

    public int? Status { get; set; }

    public DateTime? DateCreated { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
