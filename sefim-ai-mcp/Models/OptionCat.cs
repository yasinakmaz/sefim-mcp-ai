namespace SefimMcp.Models;

public class OptionCat
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int ProductId { get; set; }

    public int MaxSelections { get; set; }

    public int MinSelections { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public short? Order { get; set; }
}
