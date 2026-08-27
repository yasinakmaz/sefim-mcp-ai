namespace SefimMcp.Models;

public class PhoneOrderJson
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? TrackingNumber { get; set; }

    public string? Token { get; set; }

    public string? Json { get; set; }

    public string? Status { get; set; }

    public bool Aktarildi { get; set; }

    public string? Message { get; set; }

    public DateTime? Time { get; set; }
}
