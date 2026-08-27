namespace SefimMcp.Models;

public class CampaignHeader
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string CampaignName { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string StartTime { get; set; } = null!;

    public string EndTime { get; set; } = null!;

    public string? DaysOfWeek { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
