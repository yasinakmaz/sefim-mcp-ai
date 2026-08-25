namespace SefimMcp.Repository;

[McpServerToolType]
public class Stock (
        ISqlService<Product> productService,
        ISqlService<Choice1> choiceService,
        ISqlService<Choice2> choice2Service,
        ISqlService<Option> optionService,
        ISqlService<OptionCat> optionCatService
        ) : IStock
{
    #region Product

    [McpServerTool]
    [Description("A new product is added to 'Sefim' and the AddChoice and AddChoice2 tools must be used for product breakdowns")]
    public async ValueTask<int> AddProduct(Product product, CancellationToken cancellationToken = default)
    {
        var result = await productService.InsertAndGetIdAsync<int>(product, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Bulk-adds products to “Sefim” and returns the number of affected records")]
    public async ValueTask<int> BatchInsertProduct(List<Product> products, CancellationToken cancellationToken = default)
    {
        var result = await productService.BatchInsertAsync(products, 1000, cancellationToken);
        
        return result;
    }
    
    [McpServerTool]
    [Description("Pulls Certain Columns Of The Main Product Header Information On 'Şefim' As A List. The Search Parameter Uses The Advanced Search Feature That Assumes Fields Such As Product Name, Product Group, Product Code, Price And VAT.")]
    public async ValueTask<List<Product>> ListProduct(string search, CancellationToken cancellationToken = default)
    {
        string restorizesearch = $"%{search}%";

        var parameters = new Dictionary<string, object?>()
        {
            ["search"] = restorizesearch
        };

        var result = await productService.ExecuteRawQueryAsync(Querys.ProductListQuery, parameters, cancellationToken);

        return result.ToList();
    }
    
    [McpServerTool]
    [Description("Retrieves a single product from the 'Şefim' app. The 'Productid' parameter is required.")]
    public async ValueTask<Product> FindProduct(int productid, CancellationToken cancellationToken = default)
    {
        var result = await productService.GetByIdAsync(productid, cancellationToken);

        return result ?? new Product();
    }

    [McpServerTool]
    [Description("The 'Şefim' app will permanently delete the product, and it will be permanently removed from the database. This action is absolutely irreversible. You must absolutely ask the user for additional confirmation and must not perform the action without asking!")]
    public async ValueTask<bool> DeleteProduct(int productid, CancellationToken cancellationToken = default)
    {
        var result = await productService.DeleteAsync(productid, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("The “Şefim” app permanently deletes products via a “hard delete,” and they are permanently removed from the database. This action is absolutely irreversible. You must always ask the user for additional confirmation and never perform the action without asking!")]
    public async ValueTask<bool> BatchDeleteProduct(List<int> productids, CancellationToken cancellationToken = default)
    {
        var result = await productService.BatchDeleteAsync(productids.Cast<object>(), 1000, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("The “Şefim” app permanently deletes products via a “hard delete,” and they are permanently removed from the database. This action is absolutely irreversible. You must always ask the user for additional confirmation and never perform the action without asking!")]
    public async ValueTask<bool> UpdateProduct(Product product, CancellationToken cancellationToken = default)
    {
        var result = await productService.UpdateAsync(product, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Bulk updates the products in ‘Şefim’ The return value is an integer and returns the number of affected records. Only update the fields you want to change; write the old values to the other fields.")]
    public async ValueTask<int> BatchUpdateProduct(List<Product> products, CancellationToken cancellationToken = default)
    {
        var result = await productService.BatchUpdateAsync(products, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("It retrieves the products associated with 'Şefim' in bulk. The ID values must be presented in bulk within the list.")]
    public async ValueTask<List<Product>> ListProductIds(List<int> productids, CancellationToken cancellationToken = default)
    {
        if (productids.Count == 0)
            return [];

        var parameters = new Dictionary<string, object?>();

        var parameterNames = new List<string>();

        for (int i = 0; i < productids.Count; i++)
        {
            var parameterName = $"@ProductId{i}";

            parameterNames.Add(parameterName);
            parameters[parameterName] = productids[i];
        }

        var wherequery = $"Id IN ({string.Join(", ", parameterNames)})";

        var result = await productService.GetWhereAsync(wherequery, parameters, cancellationToken);

        return result.ToList() ?? [];
    }
    
    #endregion

    #region Choice

    [McpServerTool]
    [Description("Adds a level 1 breakdown to an existing product in 'Şefim'")]
    public async ValueTask<int> AddChoice(Choice1 choice1, CancellationToken cancellationToken = default)
    {
        var result = await choiceService.InsertAndGetIdAsync<int>(choice1, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }
    
    [McpServerTool]
    [Description("Adds a Level 1 subcategory in bulk to an existing product in 'Şefim'")]
    public async ValueTask<int> BatchInsertChoice(List<Choice1> choices, CancellationToken cancellationToken = default)
    {
        var result = await choiceService.BatchInsertAsync(choices, 1000, cancellationToken);

        return result;
    }
    
    [McpServerTool]
    [Description("Retrieves data by filtering the Level 1 breakdowns of the main product in 'Şefim' based on the product's identity value")]
    public async ValueTask<List<Choice1>> ListChoice(int productid, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?>()
        {
            ["ProductId"] = productid
        };

        const string wherequery = "ProductId = @ProductId";

        var result = await choiceService.GetWhereAsync(wherequery, parameters, cancellationToken);
        
        return result.ToList();
    }
    
    [McpServerTool]
    [Description("Retrieves a single Level 1 selection from the ‘Şefim’ app. The ‘choiceid’ parameter is required.")]
    public async ValueTask<Choice1> FindChoice(int choiceid, CancellationToken cancellationToken = default)
    {
        var result = await choiceService.GetByIdAsync(choiceid, cancellationToken);

        return result ?? new Choice1();
    }

    [McpServerTool]
    [Description("Deletes the Level 1 selection of the main product on 'Şefim' individually and returns a Boolean value.")]
    public async ValueTask<bool> DeleteChoice(int choiceid, CancellationToken cancellationToken = default)
    {
        var result = await choiceService.DeleteAsync(choiceid, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes all Level 1 selections on ‘Şefim’ in bulk and returns the number of affected records as an integer. These actions are irreversible and cannot be undone in the database; please inform the user.")]
    public async ValueTask<int> BatchDeleteChoice(List<int> choices, CancellationToken cancellationToken = default)
    {
        var result = await choiceService.BatchDeleteAsync(choices.Cast<object>(), 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Updates the Level 1 change in 'Şefim' Update by entering the new values and report the old values exactly as they are.")]
    public async ValueTask<bool> UpdateChoice(Choice1 choice1, CancellationToken cancellationToken = default)
    {
        var result = await choiceService.UpdateAsync(choice1, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("Bulk updates the Level 1 selections in 'Şefim' Enter the new values exactly as they are and report the old values as they are.")]
    public async ValueTask<int> BatchUpdateChoice(List<Choice1> choices, CancellationToken cancellationToken = default)
    {
        var result = await choiceService.BatchUpdateAsync(choices, 1000, cancellationToken);
        
        return result;
    }

    #endregion

    #region ChoiceTwo

    [McpServerTool]
    [Description("Adds a level 2 breakdown to an existing level 1 breakdown on 'Şefim'.")]
    public async ValueTask<int> AddChoice2(Choice2 choice2, CancellationToken cancellationToken = default)
    {
        var result = await choice2Service.InsertAndGetIdAsync<int>(choice2, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }
    
    [McpServerTool]
    [Description("Adds a Level 2 subcategory in bulk to an existing product in 'Şefim'")]
    public async ValueTask<int> BatchInsertChoice2(List<Choice2> choices, CancellationToken cancellationToken = default)
    {
        var result = await choice2Service.BatchInsertAsync(choices, 1000, cancellationToken);

        return result;
    }
    
    [McpServerTool]
    [Description("The Level 1 selection on ‘Şefim’ returns the Level 2 breakdowns. The Level 1 selection requires the “identity” parameter.")]
    public async ValueTask<List<Choice2>> ListChoice2(int choiceid, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?>()
        {
            ["ChoiceId"] = choiceid
        };

        const string SqlQuery = "ChoiceId = @ChoiceId";

        var result = await choice2Service.GetWhereAsync(SqlQuery, parameters, cancellationToken);
        
        return result.ToList();
    }
    
    [McpServerTool]
    [Description("Retrieves a single Level 2 selection from the ‘Şefim’ app. The ‘choice2id’ parameter is required.")]
    public async ValueTask<Choice2> FindChoice2(int choice2id, CancellationToken cancellationToken = default)
    {
        var result = await choice2Service.GetByIdAsync(choice2id, cancellationToken);

        return result ?? new Choice2();
    }

    [McpServerTool]
    [Description("This action deletes Level 2 selections in 'Şefim' and this action cannot be undone in any way from the database. Please obtain the user's confirmation before proceeding.")]
    public async ValueTask<bool> DeleteChoice2(int choice2id, CancellationToken cancellationToken = default)
    {
        var result = await choice2Service.DeleteAsync(choice2id, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("This action will delete all Level 2 selections on 'Şefim' in bulk, and this action cannot be undone in any way from the database. Please obtain the user's confirmation before proceeding.")]
    public async ValueTask<int> BatchDeleteChoice2(List<int> choices, CancellationToken cancellationToken = default)
    {
        var result = await choice2Service.BatchDeleteAsync(choices.Cast<object>(), 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Updates the Level 2 selection for 'Şefim' Please enter the new values and report the old values in the same way.")]
    public async ValueTask<bool> UpdateChoice2(Choice2 choice2, CancellationToken cancellationToken = default)
    {
        var result = await choice2Service.UpdateAsync(choice2, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Batches the Level 2 selections in 'Şefim' Please enter the new values and report the old values in the same way.")]
    public async ValueTask<int> BatchUpdateChoice2(List<Choice2> choices, CancellationToken cancellationToken = default)
    {
        var result = await choice2Service.BatchUpdateAsync(choices, 1000, cancellationToken);

        return result;
    }

    #endregion

    #region Option

    [McpServerTool]
    public async ValueTask<int> AddOption(Option option, CancellationToken cancellationToken = default)
    {
        var result = await optionService.InsertAndGetIdAsync<int>(option, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    public async ValueTask<int> BatchInsertOption(List<Option> options, CancellationToken cancellationToken = default)
    {
        var result = await optionService.BatchInsertAsync(options, 1000, cancellationToken);
        
        return result;
    }
    
    [McpServerTool]
    public async ValueTask<List<Option>> ListOption(int productid, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?>()
        {
            ["ProductId"] = productid
        };

        const string sqlquery = "ProductId = @ProductId";

        var result = await optionService.GetWhereAsync(sqlquery, parameters, cancellationToken);
        
        return result.ToList();
    }
    
    [McpServerTool]
    public async ValueTask<Option> FindOption(int optionid, CancellationToken cancellationToken = default)
    {
        var result = await optionService.GetByIdAsync(optionid, cancellationToken);

        return result ?? new Option();
    }

    [McpServerTool]
    public async ValueTask<bool> DeleteOption(int optionid, CancellationToken cancellationToken = default)
    {
        var result = await optionService.DeleteAsync(optionid, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    public async ValueTask<int> BatchDeleteOption(List<int> optionids, CancellationToken cancellationToken = default)
    {
        var result = await optionService.BatchDeleteAsync(optionids.Cast<object>(), 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    public async ValueTask<bool> UpdateOption(Option option, CancellationToken cancellationToken = default)
    {
        var result = await optionService.UpdateAsync(option, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    public async ValueTask<int> BatchUpdateOption(List<Option> options, CancellationToken cancellationToken = default)
    {
        var result = await optionService.BatchUpdateAsync(options, 1000, cancellationToken);

        return result;
    }

    #endregion

    #region OptionCat

    [McpServerTool]
    public async ValueTask<int> AddOptionCat(OptionCat optionCat, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.InsertAndGetIdAsync<int>(optionCat, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    public async ValueTask<int> BatchInsertOptionCat(List<OptionCat> optionCats, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.BatchInsertAsync(optionCats, 1000, cancellationToken);

        return result;
    }
    
    [McpServerTool]
    public async ValueTask<List<OptionCat>> ListOptionCat(int productid, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?>()
        {
            ["ProductId"] = productid
        };

        const string sqlquery = "ProductId = @ProductId";

        var result = await optionCatService.GetWhereAsync(sqlquery, parameters, cancellationToken);
        
        return result.ToList();
    }

    [McpServerTool]
    public async ValueTask<OptionCat> FindOptionCat(int optioncatid, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.GetByIdAsync(optioncatid, cancellationToken);

        return result ?? new OptionCat();
    }

    [McpServerTool]
    public async ValueTask<bool> DeleteOptionCat(int optioncatid, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.DeleteAsync(optioncatid, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    public async ValueTask<int> BatchDeleteOptionCat(List<OptionCat> optionCats, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.BatchDeleteAsync(optionCats, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    public async ValueTask<bool> UpdateOptionCat(OptionCat optionCat, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.UpdateAsync(optionCat, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    public async ValueTask<int> BatchUpdateOptionCat(List<OptionCat> optionCats, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.BatchUpdateAsync(optionCats, 1000, cancellationToken);

        return result;
    }

    #endregion
}