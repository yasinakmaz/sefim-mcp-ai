using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class ProductTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
