using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class SyncTable
{
    public long Id { get; set; }

    public long TableId { get; set; }

    public bool Aktarildi { get; set; }
}
