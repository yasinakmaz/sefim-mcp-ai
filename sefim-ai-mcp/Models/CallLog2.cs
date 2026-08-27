namespace SefimMcp.Models;

public class CallLog2
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? Phone { get; set; }

    public string? UserName { get; set; }

    public string? Line { get; set; }

    public DateTime? CallTime { get; set; }

    public DateTime? AnswerTime { get; set; }

    public string? CustomerName { get; set; }

    public string? CallId { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
