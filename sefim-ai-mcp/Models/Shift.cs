using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class Shift
{
    public DateTime Starts { get; set; }

    public DateTime? Ends { get; set; }

    public bool Aktarildi { get; set; }

    public int Id { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
