namespace SefimMcp.Models;

public class MarchingProduct
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string ProductName { get; set; } = null!;

    public int? ProductId { get; set; }
}
