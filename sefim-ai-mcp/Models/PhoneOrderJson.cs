using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class PhoneOrderJson
{
    public int Id { get; set; }

    public string? TrackingNumber { get; set; }

    public string? Token { get; set; }

    public string? Json { get; set; }

    public string? Status { get; set; }

    public bool Aktarildi { get; set; }

    public string? Message { get; set; }

    public DateTime? Time { get; set; }
}
