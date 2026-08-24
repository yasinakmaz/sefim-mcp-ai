using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public class Choice2
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Choice1Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Price { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public short? Order { get; set; }
}
