namespace SefimMcp.Models;

public class ProductImage
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public int? ProductId { get; set; }

    public string? Calory { get; set; }

    public string? ServiceTime { get; set; }

    public string? ProductDefinition { get; set; }

    public bool IsHeadPicture { get; set; }

    public bool YarimPorsiyon { get; set; }

    public string? Image { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public string? ProductGroup { get; set; }

    public string? Menu { get; set; }
}
