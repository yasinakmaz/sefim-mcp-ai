using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class DefaultOption
{
    public int Id { get; set; }

    public string? ProductName { get; set; }

    public string? Options { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
