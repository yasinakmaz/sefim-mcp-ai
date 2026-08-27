namespace SefimMcp.Models;

public class TriggerTable
{
    [PrimaryKey (IsIdentity = true)]
    public long Id { get; set; }

    public string TableName { get; set; } = null!;

    public string TableColumn { get; set; } = null!;

    public string TableId { get; set; } = null!;

    public string Command { get; set; } = null!;

    public bool Aktarildi { get; set; }
}
