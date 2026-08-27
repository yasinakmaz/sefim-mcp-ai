namespace SefimMcp.Models;

public class Tblpbipuanyetki
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public decimal? Userno { get; set; }

    public decimal? Yetki { get; set; }

    public string? Aciklama { get; set; }

    public bool Aktarildi { get; set; }
}
