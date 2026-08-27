namespace SefimMcp.Models;

public class PaymentMethod
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string PaymentGroup { get; set; } = null!;

    public string PaymentMethod1 { get; set; } = null!;

    public int PaymentAccount { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public short? VisibleType { get; set; }
}
