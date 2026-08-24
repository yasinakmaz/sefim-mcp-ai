using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class VerInfo
{
    public int? Versiyon { get; set; }

    public string? Content { get; set; }

    public byte[]? File { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
