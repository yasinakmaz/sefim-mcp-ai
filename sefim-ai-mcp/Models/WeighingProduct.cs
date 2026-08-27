namespace SefimMcp.Models;

public class WeighingProduct
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string ProductName { get; set; } = null!;

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
