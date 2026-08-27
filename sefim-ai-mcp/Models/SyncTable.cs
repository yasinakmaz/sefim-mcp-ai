namespace SefimMcp.Models;

public class SyncTable
{
    [PrimaryKey (IsIdentity = true)]
    public long Id { get; set; }

    public long TableId { get; set; }

    public bool Aktarildi { get; set; }
}
