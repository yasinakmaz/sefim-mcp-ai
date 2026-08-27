namespace SefimMcp.Models;

public class SayimDetay
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public int SayimId { get; set; }

    public int Stokind { get; set; }

    public decimal Sayim { get; set; }

    public decimal EnvanterOrg { get; set; }

    public decimal Zayi { get; set; }

    public decimal Maliyet { get; set; }

    public decimal Alisfiyati { get; set; }

    public int Kaynak { get; set; }

    public int Birimid { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
