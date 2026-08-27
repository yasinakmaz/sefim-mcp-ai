namespace SefimMcp.Models;

public class AddedCoverredWithProduct
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string ProductName { get; set; } = null!;

    public int? ProductId { get; set; }

    public bool Aktarildi { get; set; }
}
