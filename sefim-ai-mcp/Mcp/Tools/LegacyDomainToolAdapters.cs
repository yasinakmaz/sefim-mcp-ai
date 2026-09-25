namespace SefimMcp.Mcp.Tools;

[McpServerToolType]
public sealed class StockMcpTools(
    IConfiguration configuration,
    ISqlService<Product> productService,
    ISqlService<Choice1> choiceService,
    ISqlService<Choice2> choice2Service,
    ISqlService<Option> optionService,
    ISqlService<OptionCat> optionCatService,
    ISqlService<WeighingProduct> weighingProductService,
    ISqlService<ProductImage> productImageService,
    ISqlService<Menu> menuService,
    ISqlService<MenuProduct> menuProductService)
    : Stock(configuration, productService, choiceService, choice2Service, optionService, optionCatService, weighingProductService, productImageService, menuService, menuProductService);

[McpServerToolType]
public sealed class CustomerMcpTools(ISqlService<Customer> customerService) : Customers(customerService);

[McpServerToolType]
public sealed class UserMcpTools(ISqlService<User> userService, ISqlService<UserProduct> userProductService, ISqlService<Permission> permissionService)
    : Users(userService, userProductService, permissionService);

[McpServerToolType]
public sealed class TransactionMcpTools(ISqlService<DirectTransaction> directTransactionService) : Transactions(directTransactionService);

[McpServerToolType]
public sealed class TableGroupMcpTools(ISqlService<TableGroup> tableGroupService) : TableGroups(tableGroupService);

[McpServerToolType]
public sealed class CampaignMcpTools(
    ISqlService<CampaignHeader> campaignHeaderService,
    ISqlService<CampaignDetail> campaignDetailService,
    ISqlService<ProductTemplate> productTemplateService,
    ISqlService<ProductTemplatePrice> productTemplatePriceService,
    ISqlService<TemplateOverride> templateOverrideService)
    : Campaigns(campaignHeaderService, campaignDetailService, productTemplateService, productTemplatePriceService, templateOverrideService);

[McpServerToolType]
public sealed class ReportMcpTools(IConnectionFactory connectionFactory, IEntityMapper entityMapper) : Report(connectionFactory, entityMapper);
