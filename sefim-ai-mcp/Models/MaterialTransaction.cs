using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class MaterialTransaction
{
    public int Id { get; set; }

    public string? MaterialName { get; set; }

    public decimal? Quantity { get; set; }

    public DateTime Date { get; set; }

    public int TransactionType { get; set; }

    public string? Description { get; set; }

    public string? Branch { get; set; }

    public string? Department { get; set; }

    public decimal? Price { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
