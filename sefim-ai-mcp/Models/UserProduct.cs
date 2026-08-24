using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class UserProduct
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public int? ProductId { get; set; }

    public bool? Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
