using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class Tblpbipuanyetki
{
    public int Id { get; set; }

    public decimal? Userno { get; set; }

    public decimal? Yetki { get; set; }

    public string? Aciklama { get; set; }

    public bool Aktarildi { get; set; }
}
