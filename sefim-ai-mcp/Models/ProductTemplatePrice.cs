namespace SefimMcp.Models;

public class ProductTemplatePrice
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public int TemplateId { get; set; }

    public int ProductId { get; set; }

    public int Choice1Id { get; set; }

    public int Choice2Id { get; set; }

    public int OptionsId { get; set; }

    public decimal? Price { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public int? MenuId { get; set; }
}
