using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class DigiPan
{
    public int Id { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? Date { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
