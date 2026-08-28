namespace SefimMcp.Repository;

public class Stock (
        IConfiguration configuration,
        ISqlService<Product> productService,
        ISqlService<Choice1> choiceService,
        ISqlService<Choice2> choice2Service,
        ISqlService<Option> optionService,
        ISqlService<OptionCat> optionCatService,
        ISqlService<WeighingProduct> weighingProductService,
        ISqlService<ProductImage> productImageService,
        ISqlService<Menu> menuService,
        ISqlService<MenuProduct> menuProductService
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
    [Description("Adds options to a specific product on the “Şefim” app one at a time.")]
    public async ValueTask<int> AddOption(Option option, CancellationToken cancellationToken = default)
    {
        var result = await optionService.InsertAndGetIdAsync<int>(option, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Adds options to a specific product on the “Şefim” app in bulk.")]
    public async ValueTask<int> BatchInsertOption(List<Option> options, CancellationToken cancellationToken = default)
    {
        var result = await optionService.BatchInsertAsync(options, 1000, cancellationToken);
        
        return result;
    }
    
    [McpServerTool]
    [Description("It lists the options for a specific product on the “Şefim” app.")]
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
    [Description("It displays the option on the “Şefim” app individually.")]
    public async ValueTask<Option> FindOption(int optionid, CancellationToken cancellationToken = default)
    {
        var result = await optionService.GetByIdAsync(optionid, cancellationToken);

        return result ?? new Option();
    }

    [McpServerTool]
    [Description("Deletes the option for a specific product in the “Şefim” app. Warning: This action cannot be undone. Please do not perform this action without user confirmation.")]
    public async ValueTask<bool> DeleteOption(int optionid, CancellationToken cancellationToken = default)
    {
        var result = await optionService.DeleteAsync(optionid, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes all options on the “Şefim” app at once. Warning: This action cannot be undone. Please do not proceed without user confirmation.")]
    public async ValueTask<int> BatchDeleteOption(List<int> optionids, CancellationToken cancellationToken = default)
    {
        var result = await optionService.BatchDeleteAsync(optionids.Cast<object>(), 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("The options in the “Şefim” app update the options for a specific product.")]
    public async ValueTask<bool> UpdateOption(Option option, CancellationToken cancellationToken = default)
    {
        var result = await optionService.UpdateAsync(option, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("The “Şefim” app updates the options for a specific product in bulk.")]
    public async ValueTask<int> BatchUpdateOption(List<Option> options, CancellationToken cancellationToken = default)
    {
        var result = await optionService.BatchUpdateAsync(options, 1000, cancellationToken);

        return result;
    }

    #endregion

    #region OptionCat

    [McpServerTool]
    [Description("Adds an option category to a specific product in the “Şefim” app.")]
    public async ValueTask<int> AddOptionCat(OptionCat optionCat, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.InsertAndGetIdAsync<int>(optionCat, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Adds a category of options in bulk to a specific product in the “Şefim” app.")]
    public async ValueTask<int> BatchInsertOptionCat(List<OptionCat> optionCats, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.BatchInsertAsync(optionCats, 1000, cancellationToken);

        return result;
    }
    
    [McpServerTool]
    [Description("It lists the option categories for a specific product on the “Şefim” app in bulk.")]
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
    [Description("It displays a specific category of options on the “Şefim” app individually.")]
    public async ValueTask<OptionCat> FindOptionCat(int optioncatid, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.GetByIdAsync(optioncatid, cancellationToken);

        return result ?? new OptionCat();
    }

    [McpServerTool]
    [Description("Deletes a specific category of options from the “Şefim” app.")]
    public async ValueTask<bool> DeleteOptionCat(int optioncatid, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.DeleteAsync(optioncatid, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes a specific category of options from the “Şefim” app in bulk.")]
    public async ValueTask<int> BatchDeleteOptionCat(List<OptionCat> optionCats, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.BatchDeleteAsync(optionCats, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Updates a specific category of options in the “Şefim” app.")]
    public async ValueTask<bool> UpdateOptionCat(OptionCat optionCat, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.UpdateAsync(optionCat, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("Bulk updates a specific category of options in the “Şefim” app.")]
    public async ValueTask<int> BatchUpdateOptionCat(List<OptionCat> optionCats, CancellationToken cancellationToken = default)
    {
        var result = await optionCatService.BatchUpdateAsync(optionCats, 1000, cancellationToken);

        return result;
    }

    #endregion

    #region WeighingProduct

    [McpServerTool]
    [Description("Adds a “weighted product” description to the weighed product in the “Şefim” app.")]
    public async ValueTask<int> AddWeighingProduct(WeighingProduct weighingProduct, CancellationToken cancellationToken = default)
    {
        var result = await weighingProductService.InsertAndGetIdAsync<int>(weighingProduct, cancellationToken);
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Define the products in the Şefim app as weighed (by weight) items.")]
    public async ValueTask<int> BatchInsertWeighingProduct(List<WeighingProduct> weighingProduct, CancellationToken cancellationToken = default)
    {
        var result = await weighingProductService.BatchInsertAsync(weighingProduct, 1000, cancellationToken);

        return result;
    }
    
    [McpServerTool]
    [Description("Lists the weighed (by weight) products on the “Şefim” app.")]
    public async ValueTask<List<WeighingProduct>> ListWeighingProduct(CancellationToken cancellationToken = default)
    {
        var result = await weighingProductService.GetAllAsync(cancellationToken);
        
        return result.ToList() ?? new List<WeighingProduct>();
    }

    [McpServerTool]
    [Description("The “Şefim” app searches for the weighed (by weight) product by its name.")]
    public async ValueTask<List<WeighingProduct>> GetWeighingProduct(string productname, CancellationToken cancellationToken = default)
    {
        const string wherequery = "ProductName = @ProductName";

        var parameters = new Dictionary<string, object?>()
        {
            ["ProductName"] = productname
        };

        var result = await weighingProductService.GetWhereAsync(wherequery, parameters, cancellationToken);

        return result.ToList() ?? new List<WeighingProduct>();
    }

    [McpServerTool]
    [Description("Deletes the description of a weighed (gram-based) product in the “Şefim” app.")]
    public async ValueTask<bool> DeleteWeighingProduct(int id, CancellationToken cancellationToken = default)
    {
        var result = await weighingProductService.DeleteAsync(id, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Bulk-deletes the descriptions of weighed (gram-based) products in the “Şefim” app.")]
    public async ValueTask<int> BatchDeleteWeighingProduct(List<int> ids, CancellationToken cancellationToken = default)
    {
        var result = await weighingProductService.BatchDeleteAsync(ids.Cast<object>(), 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Updates the product description for weighed (by weight) items in the “Şefim” app.")]
    public async ValueTask<bool> UpdateWeighingProduct(WeighingProduct weighingProduct, CancellationToken cancellationToken = default)
    {
        var result = await weighingProductService.UpdateAsync(weighingProduct, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("Bulk-updates the descriptions of weighed (gram-based) products in the “Şefim” app.")]
    public async ValueTask<int> BatchUpdateWeighingProduct(List<WeighingProduct> weighingProducts, CancellationToken cancellationToken = default)
    {
        var result = await weighingProductService.BatchUpdateAsync(weighingProducts, 1000, cancellationToken);

        return result;
    }

    #endregion

    #region ProductImage

    private readonly string _imageLocation = configuration["SEFIM:ImageLocation"] ?? string.Empty;
    private readonly string? _imageUsername = configuration["SEFIM:ImageUsername"];
    private readonly string? _imagePassword = configuration["SEFIM:ImagePassword"];

    [McpServerTool]
    [Description("It displays a picture of a specific product on the “Şefim” app.")]
    public async ValueTask<Content.McpToolResponse> GetImage(int productid)
    {
        if (productid <= 0)
        {
            return new Content.McpToolResponse(
            [
                new Content.McpContent(Type: "text", Text: "Invalid product ID.")
            ]);
        }

        try
        {
            const string wheresql = "ProductId = @ProductId";

            var parameters = new Dictionary<string, object?>()
            {
                ["ProductId"] = productid
            };

            var result = await productImageService.GetWhereAsync(wheresql, parameters);
            var productImage = result.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(productImage?.Image))
            {
                return ImageStorageHelper.TextResponse("The image could not be found in the image database, or the “Image” column is empty.");
            }

            string imageRoot = ImageStorageHelper.NormalizeImageRoot(_imageLocation);
            string imagePath = ImageStorageHelper.BuildImagePath(imageRoot, productImage.Image);

            using var imageStorageConnection = ImageStorageHelper.OpenConnection(imageRoot, _imageUsername, _imagePassword);

            if (!File.Exists(imagePath))
            {
                return ImageStorageHelper.TextResponse($"The product with ID {productid} was found in the database but could not be located at the specified file path or could not be read. Path: {imagePath}");
            }
            
            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
            string base64Data = Convert.ToBase64String(imageBytes);

            return new Content.McpToolResponse(
            [
                new Content.McpContent(
                    Type: "text", 
                    Text: $"Image of the product with ID {productid}."
                ),
                new Content.McpContent(
                    Type: "image", 
                    Data: base64Data, 
                    MimeType: ImageStorageHelper.GetMimeType(imagePath)
                )
            ]);
        }
        catch (Exception ex)
        {
            return ImageStorageHelper.TextResponse($"An error occurred while reading the image: {ex.Message}");
        }
    }
    
    [McpServerTool]
    [Description("Adds an image to a specific product in the “Şefim” app.")]
    public async ValueTask<Content.McpToolResponse> SaveImage(
        [Description("The ID of the product to which the image belongs.")] int productid,
        [Description("If the image is on the web, enter its URL.")] string? imageUrl = null,
        [Description("If the image is pixel-based (PNG, JPG, etc.), enter the Base64 text. (Not recommended; URL is the default.)")] string? imageBase64 = null,
        [Description("If the image is a vector image (SVG), enter the XML/SVG code directly.")] string? svgContent = null)
    {
        if (productid <= 0)
        {
            return new Content.McpToolResponse(
            [
                new Content.McpContent(Type: "text", Text: "Invalid product ID.")
            ]);
        }

        try
        {
            string targetFolder = ImageStorageHelper.NormalizeImageRoot(_imageLocation);

            using var imageStorageConnection = ImageStorageHelper.OpenConnection(targetFolder, _imageUsername, _imagePassword);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string savedFileName;
            string savedFilePath;
            byte[]? imageBytesForDatabase = null;

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                string extension = ImageStorageHelper.GetImageExtensionFromUrl(imageUrl);
                savedFileName = $"{productid}_{Guid.NewGuid():N}{extension}";
                savedFilePath = Path.Combine(targetFolder, savedFileName);
                
                using var client = new HttpClient();
                byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
                imageBytesForDatabase = imageBytes;
                await File.WriteAllBytesAsync(savedFilePath, imageBytes);
            }
            else if (!string.IsNullOrWhiteSpace(imageBase64))
            {
                byte[] imageBytes = ImageStorageHelper.DecodeBase64Image(imageBase64, out string extension);
                imageBytesForDatabase = imageBytes;

                savedFileName = $"{productid}_{Guid.NewGuid():N}{extension}";
                savedFilePath = Path.Combine(targetFolder, savedFileName);
                
                await File.WriteAllBytesAsync(savedFilePath, imageBytes);
            }
            else if (!string.IsNullOrWhiteSpace(svgContent))
            {
                savedFileName = $"{productid}_{Guid.NewGuid():N}.svg";
                savedFilePath = Path.Combine(targetFolder, savedFileName);
                
                await File.WriteAllTextAsync(savedFilePath, svgContent);
            }
            else
            {
                return new Content.McpToolResponse(
                [
                    new Content.McpContent(Type: "text", Text: "Image data is missing! Please submit one of the following parameters: ‘imageUrl’, ‘imageBase64’, or ‘svgContent’.")
                ]);
            }

            var productImage = new ProductImage
            {
                Id = 0,
                ProductId = productid,
                Calory = null,
                ServiceTime = null,
                ProductDefinition = null,
                IsHeadPicture = false,
                YarimPorsiyon = false,
                Image = savedFileName,
                Aktarildi = false,
                IsSynced = false,
                IsUpdated = false,
                ProductGroup = null,
                Menu = null
            };

            var result = await productImageService.InsertAndGetIdAsync<int>(productImage);

            if (result <= 0)
            {
                return new Content.McpToolResponse(
                [
                    new Content.McpContent(Type: "text", Text: "The Image Could Not Be Saved to the Database")
                ]);
            }

            return new Content.McpToolResponse(
            [
                new Content.McpContent(
                    Type: "text", 
                    Text: $"Success! The image of the product with ID {productid} has been saved to disk ({savedFileName}), and the database save operation is complete."
                )
            ]);
        }
        catch (Exception ex)
        {
            return new Content.McpToolResponse(
            [
                new Content.McpContent(Type: "text", Text: $"An error occurred while saving the image: {ex.Message}")
            ]);
        }
    }
    
    #endregion

    #region Menu

    [McpServerTool]
    [Description("Creates a menu in the “Şefim” app.")]
    public async ValueTask<int> AddMenu(Menu menu, CancellationToken cancellationToken = default)
    {
        var result = await menuService.InsertAndGetIdAsync<int>(menu, cancellationToken);

        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("It creates a group menu in the “Şefim” app.")]
    public async ValueTask<int> BatchInsertMenu(List<Menu> menus, CancellationToken cancellationToken = default)
    {
        var result = await menuService.BatchInsertAsync(menus, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("It updates the relevant menu in the “Şefim” app.")]
    public async ValueTask<bool> UpdateMenu(Menu menu, CancellationToken cancellationToken = default)
    {
        var result = await menuService.UpdateAsync(menu, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("It applies bulk updates to the relevant menus in the “Şefim” app.")]
    public async ValueTask<int> BatchUpdateMenu(List<Menu> menus, CancellationToken cancellationToken = default)
    {
        var result = await menuService.BatchUpdateAsync(menus, 1000, cancellationToken);
        
        return result;
    }
    
    [McpServerTool]
    [Description("Deletes the relevant menu in the “Şefim” app.")]
    public async ValueTask<bool> DeleteMenu(int menuid, CancellationToken cancellationToken = default)
    {
        var result = await menuService.DeleteAsync(menuid, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes the relevant menus in bulk on the “Şefim” app.")]
    public async ValueTask<int> BatchDeleteMenu(List<int> menuids, CancellationToken cancellationToken = default)
    {
        var result = await menuService.BatchDeleteAsync(menuids.Cast<object>(), 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("It lists the relevant menus by filtering them in the “Şefim” app.")]
    public async ValueTask<List<Menu>> ListMenu(string search, CancellationToken cancellationToken = default)
    {
        string normalizesearch = $"%{search}%";
        
        var parameters = new Dictionary<string, object?>()
        {
            ["Search"] = normalizesearch.Trim() 
        };

        var result = await menuService.ExecuteRawQueryAsync(Querys.ListMenuQuery, parameters, cancellationToken);

        return result.ToList() ?? new List<Menu>();
    }

    [McpServerTool]
    [Description("It brings up the relevant menu in the “Şefim” app.")]
    public async ValueTask<Menu> GetMenu(int menuid, CancellationToken cancellationToken = default)
    {
        var result = await menuService.GetByIdAsync(menuid, cancellationToken);

        return result ?? new Menu();
    }

    #endregion

    #region MenuProduct

    [McpServerTool]
    [Description("Adds a single product to the relevant menu in the “Şefim” app.")]
    public async ValueTask<int> AddMenuProduct(MenuProduct menuProduct, CancellationToken cancellationToken = default)
    {
        var result = await menuProductService.InsertAndGetIdAsync<int>(menuProduct, cancellationToken);

        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Adds products in bulk to the relevant menu in the “Şefim” app.")]
    public async ValueTask<int> BatchInsertMenuProduct(List<MenuProduct> menuProducts, CancellationToken cancellationToken = default)
    {
        var result = await menuProductService.BatchInsertAsync(menuProducts, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Updates the product in the relevant menu on the “Şefim” app.")]
    public async ValueTask<bool> UpdateMenuProduct(MenuProduct menuProduct, CancellationToken cancellationToken = default)
    {
        var result = await menuProductService.UpdateAsync(menuProduct, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("It updates the relevant menu items in bulk on the “Şefim” app.")]
    public async ValueTask<int> BatchUpdateMenuProduct(List<MenuProduct> menuProducts, CancellationToken cancellationToken = default)
    {
        var result = await menuProductService.BatchUpdateAsync(menuProducts, 1000, cancellationToken);
        
        return result;
    }

    [McpServerTool]
    [Description("Deletes the relevant menu item in the “Şefim” app.")]
    public async ValueTask<bool> DeleteMenuProduct(int menuProductid, CancellationToken cancellationToken = default)
    {
        var result = await menuProductService.DeleteAsync(menuProductid, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes all items from the relevant menus in the “Şefim” app in one go.")]
    public async ValueTask<int> BatchDeleteMenuProduct(List<int> menuProductids, CancellationToken cancellationToken = default)
    {
        var result = await menuProductService.BatchDeleteAsync(menuProductids.Cast<object>(), 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("It lists menu items in the “Şefim” app.")]
    public async ValueTask<List<MenuProduct>> ListMenuProduct(string search, CancellationToken cancellationToken = default)
    {
        string normalizesearch = $"%{search}%";
        
        var parameters = new Dictionary<string, object?>()
        {
            ["Search"] = normalizesearch.Trim() 
        };

        var result = await menuProductService.ExecuteRawQueryAsync(Querys.ListMenuProductQuery, parameters, cancellationToken);

        return result.ToList() ?? new List<MenuProduct>();
    }

    [McpServerTool]
    [Description("It retrieves the menu item from the “Şefim” app.")]
    public async ValueTask<MenuProduct> GetMenuProduct(int menuProductid, CancellationToken cancellationToken = default)
    {
        var result = await menuProductService.GetByIdAsync(menuProductid, cancellationToken);

        return result ?? new MenuProduct();
    }

    [McpServerTool]
    [Description("Retrieves the items from a specific menu in the “Şefim” app.")]
    public async ValueTask<List<MenuProduct>> GetMenuProductByMenuId(int menuId, CancellationToken cancellationToken = default)
    {
        const string whereQuery = "MenuId = @MenuId";

        var parameters = new Dictionary<string, object?>()
        {
            ["MenuId"] = menuId
        };

        var result = await menuProductService.GetWhereAsync(whereQuery, parameters, cancellationToken);

        return result.ToList() ?? new List<MenuProduct>();
    }

    #endregion

}
