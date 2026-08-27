namespace SefimMcp.Models;

public class Tblpospuanaciklama
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public int Pind { get; set; }

    public string Aciklama { get; set; } = null!;

    public bool Aktarildi { get; set; }
}
