namespace SefimMcp.Interfaces;

public interface ICampaign
{
    // Campaign Header
    
    public ValueTask<int> AddCampaign(CampaignHeader campaignHeader, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkInsertCampaign(List<CampaignHeader> campaignHeader, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateCampaign(CampaignHeader campaignHeader, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkUpdateCampaign(List<CampaignHeader> campaignHeader, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteCampaign(int id, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkDeleteCampaign(List<CampaignHeader> campaignHeader, CancellationToken cancellationToken = default);
    
    public ValueTask<List<CampaignHeader>> GetCampaigns(CancellationToken cancellationToken = default);
    
    public ValueTask<CampaignHeader> GetCampaign(int id, CancellationToken cancellationToken = default);
    
    // Campaign Detail
    
    public ValueTask<int> AddCampaignDetail(CampaignDetail campaignDetailHeader, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkInsertCampaignDetail(List<CampaignDetail> campaignDetailHeader, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateCampaignDetail(CampaignDetail campaignDetailHeader, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkUpdateCampaignDetail(List<CampaignDetail> campaignDetailHeader, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteCampaignDetail(int id, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkDeleteCampaignDetail(List<CampaignDetail> campaignDetailHeader, CancellationToken cancellationToken = default);
    
    public ValueTask<List<CampaignDetail>> GetCampaignDetails(int campaignHeaderId, CancellationToken cancellationToken = default);
    
    public ValueTask<CampaignDetail> GetCampaignDetail(int id, CancellationToken cancellationToken = default);
    
    // Product Template
    
    public ValueTask<int> AddProductTemplate(ProductTemplate productTemplate, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkInsertProductTemplate(List<ProductTemplate> productTemplate, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateProductTemplate(ProductTemplate productTemplate, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkUpdateProductTemplate(List<ProductTemplate> productTemplate, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteProductTemplate(int id, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkDeleteProductTemplate(List<ProductTemplate> productTemplate, CancellationToken cancellationToken = default);
    
    public ValueTask<List<ProductTemplate>> GetProductTemplates(int productTemplateId, CancellationToken cancellationToken = default);
    
    public ValueTask<ProductTemplate> GetProductTemplate(int id, CancellationToken cancellationToken = default);
    
    // Product Template Price
    
    public ValueTask<int> AddProductTemplatePrice(ProductTemplatePrice productTemplatePrice, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkInsertProductTemplatePrice(List<ProductTemplatePrice> productTemplatePrice, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateProductTemplatePrice(ProductTemplatePrice productTemplatePrice, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkUpdateProductTemplatePrice(List<ProductTemplatePrice> productTemplatePrice, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteProductTemplatePrice(int id, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkDeleteProductTemplatePrice(List<ProductTemplatePrice> productTemplatePrice, CancellationToken cancellationToken = default);
    
    public ValueTask<List<ProductTemplatePrice>> GetProductTemplatePrices(int productTemplatePriceId, CancellationToken cancellationToken = default);
    
    public ValueTask<ProductTemplatePrice> GetProductTemplatePrice(int id, CancellationToken cancellationToken = default);
}