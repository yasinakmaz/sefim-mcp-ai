using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class AppVer
{
    public string Name { get; set; } = null!;

    public string? Value { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public int Id { get; set; }
}
