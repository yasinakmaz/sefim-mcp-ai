using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class MarchingProduct
{
    public int Id { get; set; }

    public string ProductName { get; set; } = null!;

    public int? ProductId { get; set; }
}
