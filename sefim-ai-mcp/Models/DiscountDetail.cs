using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class DiscountDetail
{
    public int Id { get; set; }

    public string ProductGroup { get; set; } = null!;

    public decimal DiscountPercentage { get; set; }

    public decimal Total { get; set; }

    public int HeaderId { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
