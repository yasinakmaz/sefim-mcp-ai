using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class CallLog
{
    public int Id { get; set; }

    public string? Phone { get; set; }

    public string? Server { get; set; }

    public string? Line { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
