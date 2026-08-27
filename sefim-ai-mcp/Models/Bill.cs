namespace SefimMcp.Models;

public class Bill
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string UserName { get; set; } = null!;

    public string? ProductName { get; set; }

    public int? ProductId { get; set; }

    public int? Choice1Id { get; set; }

    public int? Choice2Id { get; set; }

    public string? Options { get; set; }

    public decimal? Price { get; set; }

    public decimal? Quantity { get; set; }

    public string? Comment { get; set; }

    public int HeaderId { get; set; }

    public int OrderId { get; set; }

    public int? CampaignDetailId { get; set; }

    public bool? CampaignFree { get; set; }

    public bool? CampaignSold { get; set; }

    public bool? Canceling { get; set; }

    public decimal? OriginalPrice { get; set; }

    public bool? Printed { get; set; }

    public int? OrderState { get; set; }

    public int IsReady { get; set; }

    public int IsSee { get; set; }

    public DateTime? ReadyDate { get; set; }

    public string? UserK { get; set; }

    public string? ExtOrderNo { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public bool? Zayi { get; set; }

    public bool? Ikram { get; set; }

    public string? MenuName { get; set; }

    public int? MenuSiraNo { get; set; }

    public short? AktarimDurumu { get; set; }

    public string? CampaingGuid { get; set; }
}
