using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class Tblpospuan
{
    public int Id { get; set; }

    public DateTime? Tarih { get; set; }

    public string? Belgeno { get; set; }

    public int? Cariind { get; set; }

    public decimal? Puan { get; set; }

    public int? Belgetipi { get; set; }

    public int? Belgeind { get; set; }

    public int? Aktarildi { get; set; }

    public bool Aktarildi1 { get; set; }
}
