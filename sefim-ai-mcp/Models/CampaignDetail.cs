using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class CampaignDetail
{
    public int Id { get; set; }

    public int CampaignHeaderId { get; set; }

    public int SoldProductId { get; set; }

    public int? SoldChoice1Id { get; set; }

    public int? SoldChoice2Id { get; set; }

    public decimal SoldQuantity { get; set; }

    public int FreeProductId { get; set; }

    public int? FreeChoice1Id { get; set; }

    public int? FreeChoice2Id { get; set; }

    public decimal FreeQuantity { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public short? FreePercent { get; set; }
}
