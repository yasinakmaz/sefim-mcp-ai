using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class Permission
{
    public string UserName { get; set; } = null!;

    public string PermissionName { get; set; } = null!;

    public string? PermissionValue { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public int Id { get; set; }
}
