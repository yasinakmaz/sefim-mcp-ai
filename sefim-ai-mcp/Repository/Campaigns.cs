namespace SefimMcp.Repository;

public class Campaigns (
        ISqlService<CampaignHeader> campaignHeaderService,
        ISqlService<CampaignDetail> campaignDetailService,
        ISqlService<ProductTemplate> productTemplateService,
        ISqlService<ProductTemplatePrice> productTemplatePriceService,
        ISqlService<TemplateOverride> templateOverrideService
        ) : ICampaign
{
    #region Campaign Header

    [McpServerTool]
    [Description("Creates one campaign header. The campaign becomes effective according to its own date and rule fields, so confirm those before writing.")]
    public async ValueTask<int> AddCampaign(CampaignHeader campaignHeader, CancellationToken cancellationToken = default)
    {
        var result = await campaignHeaderService.InsertAndGetIdAsync<int>(campaignHeader, cancellationToken);

        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Creates many campaign headers in one batch. Bulk writes require explicit user confirmation.")]
    public async ValueTask<int> BulkInsertCampaign(List<CampaignHeader> campaignHeader, CancellationToken cancellationToken = default)
    {
        var result = await campaignHeaderService.BatchInsertAsync(campaignHeader, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Updates one campaign header. Send the complete entity: Şefim updates replace the row, they are not partial updates.")]
    public async ValueTask<bool> UpdateCampaign(CampaignHeader campaignHeader, CancellationToken cancellationToken = default)
    {
        var result = await campaignHeaderService.UpdateAsync(campaignHeader, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Updates many campaign headers in one batch. Bulk writes require explicit user confirmation.")]
    public async ValueTask<int> BulkUpdateCampaign(List<CampaignHeader> campaignHeader, CancellationToken cancellationToken = default)
    {
        var result = await campaignHeaderService.BatchUpdateAsync(campaignHeader, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Deletes one campaign header by identifier. Deletion is permanent and detail rows may become orphaned; confirm with the user first.")]
    public async ValueTask<bool> DeleteCampaign(int id, CancellationToken cancellationToken = default)
    {
        var result = await campaignHeaderService.DeleteAsync(id, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes many campaign headers in one batch. Deletion is permanent; bulk deletes require explicit user confirmation.")]
    public async ValueTask<int> BulkDeleteCampaign(List<CampaignHeader> campaignHeader, CancellationToken cancellationToken = default)
    {
        var result = await campaignHeaderService.BatchDeleteAsync(campaignHeader, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Returns every campaign header. Use it to discover campaign identifiers before reading or changing details.")]
    public async ValueTask<List<CampaignHeader>> GetCampaigns(CancellationToken cancellationToken = default)
    {
        var result = await campaignHeaderService.GetAllAsync(cancellationToken);

        return result.ToList() ?? new List<CampaignHeader>(); 
    }

    [McpServerTool]
    [Description("Returns one campaign header by identifier.")]
    public async ValueTask<CampaignHeader> GetCampaign(int id, CancellationToken cancellationToken = default)
    {
        var result = await campaignHeaderService.GetByIdAsync(id, cancellationToken);
        
        return result ?? new CampaignHeader();
    }

    #endregion

    #region Campaign Detail

    [McpServerTool]
    [Description("Creates one campaign detail line under an existing campaign header.")]
    public async ValueTask<int> AddCampaignDetail(CampaignDetail campaignDetailHeader, CancellationToken cancellationToken = default)
    {
        var result = await campaignDetailService.InsertAndGetIdAsync<int>(campaignDetailHeader, cancellationToken);

        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Creates many campaign detail lines in one batch. Bulk writes require explicit user confirmation.")]
    public async ValueTask<int> BulkInsertCampaignDetail(List<CampaignDetail> campaignDetailHeader, CancellationToken cancellationToken = default)
    {
        var result = await campaignDetailService.BatchInsertAsync(campaignDetailHeader, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Updates one campaign detail line. Send the complete entity: Şefim updates replace the row.")]
    public async ValueTask<bool> UpdateCampaignDetail(CampaignDetail campaignDetailHeader, CancellationToken cancellationToken = default)
    {
        var result = await campaignDetailService.UpdateAsync(campaignDetailHeader, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Updates many campaign detail lines in one batch. Bulk writes require explicit user confirmation.")]
    public async ValueTask<int> BulkUpdateCampaignDetail(List<CampaignDetail> campaignDetailHeader, CancellationToken cancellationToken = default)
    {
        var result = await campaignDetailService.BatchUpdateAsync(campaignDetailHeader, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Deletes one campaign detail line by identifier. Deletion is permanent; confirm with the user first.")]
    public async ValueTask<bool> DeleteCampaignDetail(int id, CancellationToken cancellationToken = default)
    {
        var result = await campaignDetailService.DeleteAsync(id, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes many campaign detail lines in one batch. Deletion is permanent; bulk deletes require explicit user confirmation.")]
    public async ValueTask<int> BulkDeleteCampaignDetail(List<CampaignDetail> campaignDetailHeader, CancellationToken cancellationToken = default)
    {
        var result = await campaignDetailService.BatchDeleteAsync(campaignDetailHeader, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Returns the detail lines of one campaign header.")]
    public async ValueTask<List<CampaignDetail>> GetCampaignDetails(int campaignHeaderId, CancellationToken cancellationToken = default)
    {
        const string whereQuery = "CampaignHeaderId = @CampaignHeaderId";

        var parameters = new Dictionary<string, object?>()
        {
            ["CampaignHeaderId"] = campaignHeaderId
        };
        
        var result = await campaignDetailService.GetWhereAsync(whereQuery, parameters, cancellationToken);

        return result.ToList() ?? new List<CampaignDetail>();
    }

    [McpServerTool]
    [Description("Returns one campaign detail line by identifier.")]
    public async ValueTask<CampaignDetail> GetCampaignDetail(int id, CancellationToken cancellationToken = default)
    {
        var result = await campaignDetailService.GetByIdAsync(id, cancellationToken);

        return result ?? new CampaignDetail();
    }

    #endregion

    #region ProductTemplate

    [McpServerTool]
    [Description("Adds a pricing template to the “Şefim” app.")]
    public async ValueTask<int> AddProductTemplate(ProductTemplate productTemplate, CancellationToken cancellationToken = default)
    {
        var result = await productTemplateService.InsertAndGetIdAsync<int>(productTemplate, cancellationToken);

        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Adds a bulk price template to the “Şefim” app.")]
    public async ValueTask<int> BulkInsertProductTemplate(List<ProductTemplate> productTemplate, CancellationToken cancellationToken = default)
    {
        var result = await productTemplateService.BatchInsertAsync(productTemplate, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Updates the relevant pricing template in the “Şefim” app.")]
    public async ValueTask<bool> UpdateProductTemplate(ProductTemplate productTemplate, CancellationToken cancellationToken = default)
    {
        var result = await productTemplateService.UpdateAsync(productTemplate, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Bulk updates the relevant pricing templates in the “Şefim” app.")]
    public async ValueTask<int> BulkUpdateProductTemplate(List<ProductTemplate> productTemplate, CancellationToken cancellationToken = default)
    {
        var result = await productTemplateService.BatchUpdateAsync(productTemplate, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Deletes the relevant pricing template from the “Şefim” app.")]
    public async ValueTask<bool> DeleteProductTemplate(int id, CancellationToken cancellationToken = default)
    {
        var result = await productTemplateService.DeleteAsync(id, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes the relevant pricing templates on the “Şefim” app in bulk.")]
    public async ValueTask<int> BulkDeleteProductTemplate(List<ProductTemplate> productTemplate, CancellationToken cancellationToken = default)
    {
        var result = await productTemplateService.BatchDeleteAsync(productTemplate, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Displays the price templates in the ‘Şefim’ app as a list.")]
    public async ValueTask<List<ProductTemplate>> GetProductTemplates(int productTemplateId, CancellationToken cancellationToken = default)
    {
        var result = await productTemplateService.GetAllAsync(cancellationToken);

        return result.ToList() ?? new List<ProductTemplate>();
    }

    [McpServerTool]
    [Description("Retrieves the relevant pricing template from the “Şefim” app.")]
    public async ValueTask<ProductTemplate> GetProductTemplate(int id, CancellationToken cancellationToken = default)
    {
        var result = await productTemplateService.GetByIdAsync(id, cancellationToken);

        return result ?? new ProductTemplate();
    }

    #endregion

    #region ProductTemplatePrice

    [McpServerTool]
    [Description("Adds a product to the relevant pricing template in the “Şefim” app.")]
    public async ValueTask<int> AddProductTemplatePrice(ProductTemplatePrice productTemplatePrice, CancellationToken cancellationToken = default)
    {
        var result = await productTemplatePriceService.InsertAndGetIdAsync<int>(productTemplatePrice, cancellationToken);

        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("It adds products in bulk to the relevant pricing templates in the “Şefim” app.")]
    public async ValueTask<int> BulkInsertProductTemplatePrice(List<ProductTemplatePrice> productTemplatePrice, CancellationToken cancellationToken = default)
    {
        var result = await productTemplatePriceService.BatchInsertAsync(productTemplatePrice, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Updates the product in the relevant pricing template in the “Şefim” app")]
    public async ValueTask<bool> UpdateProductTemplatePrice(ProductTemplatePrice productTemplatePrice, CancellationToken cancellationToken = default)
    {
        var result = await productTemplatePriceService.UpdateAsync(productTemplatePrice, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Bulk updates products in the relevant pricing templates on the “Şefim” app.")]
    public async ValueTask<int> BulkUpdateProductTemplatePrice(List<ProductTemplatePrice> productTemplatePrice, CancellationToken cancellationToken = default)
    {
        var result = await productTemplatePriceService.BatchUpdateAsync(productTemplatePrice, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Deletes the product from the relevant pricing template in the “Şefim” app.")]
    public async ValueTask<bool> DeleteProductTemplatePrice(int id, CancellationToken cancellationToken = default)
    {
        var result = await productTemplatePriceService.DeleteAsync(id, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes all products in the relevant pricing templates in the “Şefim” app in bulk.")]
    public async ValueTask<int> BulkDeleteProductTemplatePrice(List<ProductTemplatePrice> productTemplatePrice, CancellationToken cancellationToken = default)
    {
        var result = await productTemplatePriceService.BatchDeleteAsync(productTemplatePrice, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("It displays the products listed in the relevant pricing template within the “Şefim” app.")]
    public async ValueTask<List<ProductTemplatePrice>> GetProductTemplatePrices(int productTemplatePriceId, CancellationToken cancellationToken = default)
    {
        const string whereQuery = "CampaignHeaderId = @CampaignHeaderId";

        var parameters = new Dictionary<string, object?>()
        {
            ["CampaignHeaderId"] = productTemplatePriceId
        };
        
        var result = await productTemplatePriceService.GetWhereAsync(whereQuery, parameters, cancellationToken);

        return result.ToList() ?? new List<ProductTemplatePrice>();
    }

    [McpServerTool]
    [Description("It retrieves the product from the relevant pricing template in the “Şefim” app.")]
    public async ValueTask<ProductTemplatePrice> GetProductTemplatePrice(int id, CancellationToken cancellationToken = default)
    {
        var result = await productTemplatePriceService.GetByIdAsync(id, cancellationToken);

        return result ?? new ProductTemplatePrice();
    }

    #endregion

    #region Template Override

    [McpServerTool]
    [Description("Adds a template override to the “Şefim” app.")]
    public async ValueTask<int> AddTemplateOverride(TemplateOverride templateOverride, CancellationToken cancellationToken = default)
    {
        var result = await templateOverrideService.InsertAndGetIdAsync<int>(templateOverride, cancellationToken);

        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Adds template overrides in bulk to the “Şefim” app.")]
    public async ValueTask<int> BulkInsertTemplateOverrides(List<TemplateOverride> templateOverrides, CancellationToken cancellationToken = default)
    {
        var result = await templateOverrideService.BatchInsertAsync(templateOverrides, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Updates the relevant template override in the “Şefim” app.")]
    public async ValueTask<bool> UpdateTemplateOverride(TemplateOverride templateOverride, CancellationToken cancellationToken = default)
    {
        var result = await templateOverrideService.UpdateAsync(templateOverride, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Bulk updates the relevant template overrides in the “Şefim” app.")]
    public async ValueTask<int> BulkUpdateTemplateOverrides(List<TemplateOverride> templateOverrides, CancellationToken cancellationToken = default)
    {
        var result = await templateOverrideService.BatchUpdateAsync(templateOverrides, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Deletes the relevant template override in the “Şefim” app.")]
    public async ValueTask<bool> DeleteTemplateOverride(int id, CancellationToken cancellationToken = default)
    {
        var result = await templateOverrideService.DeleteAsync(id, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes the relevant template overrides on the “Şefim” app in bulk.")]
    public async ValueTask<int> BulkDeleteTemplateOverrides(List<TemplateOverride> templateOverrides, CancellationToken cancellationToken = default)
    {
        var result = await templateOverrideService.BatchDeleteAsync(templateOverrides, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Displays a list of template overrides in the “Şefim” app.")]
    public async ValueTask<List<TemplateOverride>> GetTemplateOverrides(CancellationToken cancellationToken = default)
    {
        var result = await templateOverrideService.GetAllAsync(cancellationToken);
        
        return result.ToList() ?? new List<TemplateOverride>();
    }

    [McpServerTool]
    [Description("Retrieves the relevant template override from the ‘Şefim’ app.")]
    public async ValueTask<TemplateOverride> GetTemplateOverride(int id, CancellationToken cancellationToken = default)
    {
        var result = await templateOverrideService.GetByIdAsync(id, cancellationToken);
        return result ?? new TemplateOverride();
    }

    #endregion
}