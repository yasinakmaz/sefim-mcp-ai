namespace SefimMcp.Models;

public class UserProduct
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? UserName { get; set; }

    public int? ProductId { get; set; }

    public bool? Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
