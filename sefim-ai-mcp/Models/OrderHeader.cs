using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class OrderHeader
{
    public int Id { get; set; }

    public DateTime? Date { get; set; }

    public string UserName { get; set; } = null!;

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
