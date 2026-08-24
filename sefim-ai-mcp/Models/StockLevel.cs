using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class StockLevel
{
    public string ProductName { get; set; } = null!;

    public decimal? CriticalLevel { get; set; }

    public decimal? Inventory { get; set; }

    public int? LastBillId { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public int Id { get; set; }
}
