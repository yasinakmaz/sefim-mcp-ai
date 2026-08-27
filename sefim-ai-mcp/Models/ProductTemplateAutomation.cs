namespace SefimMcp.Models;

public class ProductTemplateAutomation
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }

    public string? TemplateName { get; set; }

    public string? TimeStart { get; set; }

    public string? TimeEnd { get; set; }

    public string? DaysOfWeek { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
