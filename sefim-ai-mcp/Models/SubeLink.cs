namespace SefimMcp.Models;

public class SubeLink
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string SubeAdi { get; set; } = null!;

    public string Sunucu { get; set; } = null!;

    public string Parola { get; set; } = null!;

    public string? FiyatSablonu { get; set; }

    public string Veritabani { get; set; } = null!;

    public string Kullanici { get; set; } = null!;

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
