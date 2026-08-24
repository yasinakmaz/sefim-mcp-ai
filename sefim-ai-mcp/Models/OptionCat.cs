using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class OptionCat
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int ProductId { get; set; }

    public int MaxSelections { get; set; }

    public int MinSelections { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public short? Order { get; set; }
}
