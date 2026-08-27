namespace SefimMcp.Models;

public class SayimBaslik
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public DateTime SayimTarihi { get; set; }

    public bool SayimSonlandi { get; set; }

    public int SayimGonderildi { get; set; }

    public int GirisBelgeNo { get; set; }

    public int CikisBelgeNo { get; set; }

    public int FirmaId { get; set; }

    public int DonemId { get; set; }

    public int DepoId { get; set; }

    public int SubeId { get; set; }

    public int KasaId { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
