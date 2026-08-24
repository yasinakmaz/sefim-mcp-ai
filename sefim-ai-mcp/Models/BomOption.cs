using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class BomOption
{
    public int Id { get; set; }

    public int? OptionsId { get; set; }

    public string? OptionsName { get; set; }

    public string? MaterialName { get; set; }

    public decimal Quantity { get; set; }

    public string? Unit { get; set; }

    public int? StokId { get; set; }

    public string? ProductName { get; set; }

    public bool Aktarildi { get; set; }

    public bool? MaliyetDahil { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public string? Menu { get; set; }

    public string? OptionsName2 { get; set; }
}
