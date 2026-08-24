using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class TableLock
{
    public string TableNumber { get; set; } = null!;

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public int Id { get; set; }

    public string? UserName { get; set; }

    public bool? Locked { get; set; }
}
