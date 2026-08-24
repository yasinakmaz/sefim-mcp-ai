using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class User
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? ProductTemplate { get; set; }

    public string? Branch { get; set; }

    public string? Department { get; set; }

    public decimal? DiscRate { get; set; }

    public decimal? DiscAmount { get; set; }

    public bool? Deleted { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public string? DrawerPort { get; set; }

    public string? Role { get; set; }
}
