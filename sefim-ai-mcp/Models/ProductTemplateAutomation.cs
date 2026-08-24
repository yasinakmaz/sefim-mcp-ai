using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class ProductTemplateAutomation
{
    public int Id { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }

    public string? TemplateName { get; set; }

    public string? TimeStart { get; set; }

    public string? TimeEnd { get; set; }

    public string? DaysOfWeek { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
