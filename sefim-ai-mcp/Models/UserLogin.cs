namespace SefimMcp.Models;

public class UserLogin
{
    public string? UserName { get; set; }

    public string? MachineName { get; set; }

    public string? IpAddress { get; set; }

    public DateTime? Login { get; set; }

    public DateTime? Logout { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public bool? Aktarildi { get; set; }
}
