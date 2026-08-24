using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class Cost
{
    public int Id { get; set; }

    public int CountId { get; set; }

    public string? ProductName { get; set; }

    public string? Options { get; set; }

    public decimal Cost1 { get; set; }

    public decimal BomCost { get; set; }

    public DateOnly CostStart { get; set; }

    public DateOnly CostEnd { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
