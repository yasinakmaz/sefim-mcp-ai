using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class PaketEntegrasyon
{
    public int Id { get; set; }

    public short? PaketType { get; set; }

    public string? CompanyName { get; set; }

    public string? TySaticiId { get; set; }

    public string? TyApiKey { get; set; }

    public string? TyApiSecret { get; set; }

    public string? TySubeKodu { get; set; }

    public string? TyEposta { get; set; }

    public string? MyZincirId { get; set; }

    public string? MyApiKey { get; set; }

    public string? MyRestoranId { get; set; }

    public string? YsUserName { get; set; }

    public string? YsPassword { get; set; }

    public string? YsCatalogName { get; set; }

    public string? YsCategoryName { get; set; }

    public string? YsChaincode { get; set; }

    public string? YsRestaurantId { get; set; }

    public string? GtRestaurantSk { get; set; }

    public string? GtToken { get; set; }

    public DateTime? GtTokenEndTime { get; set; }

    public string? TyToken { get; set; }

    public DateTime? TyTokenEndTime { get; set; }

    public string? MyToken { get; set; }

    public DateTime? MyTokenEndTime { get; set; }

    public string? YsToken { get; set; }

    public DateTime? YsTokenEndTime { get; set; }

    public string? GtRestaurantId { get; set; }

    public string? YsRemoteId { get; set; }

    public string? YsId { get; set; }
}
