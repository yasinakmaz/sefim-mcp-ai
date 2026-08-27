namespace SefimMcp.Models;

public class TemplateOverride
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string TemplateName { get; set; } = null!;

    public string CustomerCategory { get; set; } = null!;

    public string TableGroup { get; set; } = null!;

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
