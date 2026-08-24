using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class Tblpospuanaciklama
{
    public int Id { get; set; }

    public int Pind { get; set; }

    public string Aciklama { get; set; } = null!;

    public bool Aktarildi { get; set; }
}
