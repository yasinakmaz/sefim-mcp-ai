namespace SefimMcp.Repository;

[McpServerToolType]
public class Stock (
        SqlService<Product> productService,
        SqlService<Choice1> choiceService,
        SqlService<Choice2> choice2Service,
        SqlService<Option> optionService,
        SqlService<OptionCat> optionCatService
        ) : IStock
{
    [McpServerTool]
    [Description("A new product is added to 'Sefim' and the AddChoice and AddChoice2 tools must be used for product breakdowns")]
    public async ValueTask<int> AddProduct(Product product, CancellationToken cancellationToken = default)
    {
        var result = await productService.InsertAndGetIdAsync<int>(product, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Adds a level 1 breakdown to an existing product in 'Şefim'")]
    public async ValueTask<int> AddChoice(Choice1 choice1, CancellationToken cancellationToken = default)
    {
        var result = await choiceService.InsertAndGetIdAsync<int>(choice1, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Adds a level 2 breakdown to an existing level 1 breakdown on 'Şefim'.")]
    public async ValueTask<int> AddChoice2(Choice2 choice2, CancellationToken cancellationToken = default)
    {
        var result = await choice2Service.InsertAndGetIdAsync<int>(choice2, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    public async ValueTask<int> AddOption(Option option, CancellationToken cancellationToken = default)
    {
        var result = await optionService.InsertAndGetIdAsync<int>(option, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    public async ValueTask<int> AddOptionCat(OptionCat optionCat, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.InsertAndGetIdAsync<int>(optionCat, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }
    
    
}