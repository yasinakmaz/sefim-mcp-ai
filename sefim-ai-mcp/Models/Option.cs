using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class Option
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Price { get; set; }

    public bool? Quantitative { get; set; }

    public int ProductId { get; set; }

    public string Category { get; set; } = null!;

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public short? Order { get; set; }
}
