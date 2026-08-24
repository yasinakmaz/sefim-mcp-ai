using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class TeraPosDef
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string IpAddress { get; set; } = null!;

    public int Port { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
