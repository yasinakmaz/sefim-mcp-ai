using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class Value
{
    public string Name { get; set; } = null!;

    public string? Value1 { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public int Id { get; set; }
}
