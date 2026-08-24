using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class EfaturaDurumKodlari
{
    public int Id { get; set; }

    public int? Kod { get; set; }

    public string? Durum { get; set; }
}
