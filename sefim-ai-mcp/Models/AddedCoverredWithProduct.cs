using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class AddedCoverredWithProduct
{
    public int Id { get; set; }

    public string ProductName { get; set; } = null!;

    public int? ProductId { get; set; }

    public bool Aktarildi { get; set; }
}
