using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class TableGroup
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Prefix { get; set; }

    public int? TableCount { get; set; }

    public string? Settings { get; set; }

    public int? ChairCount { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
