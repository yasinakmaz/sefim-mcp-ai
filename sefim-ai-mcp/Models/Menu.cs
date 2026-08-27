namespace SefimMcp.Models;

public class Menu
{
    [PrimaryKey (IsIdentity = true)]    
    public int Id { get; set; }

    public string MenuName { get; set; } = null!;

    public int ChoiceCount { get; set; }

    public decimal Price { get; set; }

    public bool Active { get; set; }

    public bool? Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
