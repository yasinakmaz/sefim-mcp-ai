using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class TriggerTable
{
    public long Id { get; set; }

    public string TableName { get; set; } = null!;

    public string TableColumn { get; set; } = null!;

    public string TableId { get; set; } = null!;

    public string Command { get; set; } = null!;

    public bool Aktarildi { get; set; }
}
