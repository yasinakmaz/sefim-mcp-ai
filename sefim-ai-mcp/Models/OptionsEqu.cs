namespace SefimMcp.Models;

public class OptionsEqu
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? ProductName { get; set; }

    public string? Options { get; set; }

    public string? EquProduct { get; set; }

    public decimal? MenuFiyat { get; set; }

    public decimal? Miktar { get; set; }

    public bool Aktarildi { get; set; }

    public bool? AnaUrun { get; set; }

    public decimal? MenuYuzde { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
