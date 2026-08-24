using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class FormalBillPrinter
{
    public int Id { get; set; }

    public string? PrinterName { get; set; }

    public string? UserName { get; set; }

    public string? ComputerName { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
