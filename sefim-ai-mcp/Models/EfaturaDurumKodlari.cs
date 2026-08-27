namespace SefimMcp.Models;

public class EfaturaDurumKodlari
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public int? Kod { get; set; }

    public string? Durum { get; set; }
}
