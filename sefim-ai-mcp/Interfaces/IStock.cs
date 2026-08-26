namespace SefimMcp.Interfaces;

public interface IStock
{
    // Product
    public ValueTask<int> AddProduct(Product product, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchInsertProduct(List<Product> products, CancellationToken cancellationToken = default);
    
    public ValueTask<List<Product>> ListProduct(string search, CancellationToken cancellationToken = default);
    
    public ValueTask<Product> FindProduct(int productid, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteProduct(int productid, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> BatchDeleteProduct(List<int> productids, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateProduct(Product product, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchUpdateProduct(List<Product> products, CancellationToken cancellationToken = default);
    
    public ValueTask<List<Product>> ListProductIds(List<int> productids, CancellationToken cancellationToken = default);
    
    // Choice
    public ValueTask<int> AddChoice(Choice1 choice1, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchInsertChoice(List<Choice1> choices, CancellationToken cancellationToken = default);
    
    public ValueTask<List<Choice1>> ListChoice(int productid, CancellationToken cancellationToken = default);
    
    public ValueTask<Choice1> FindChoice(int choiceid, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteChoice(int choiceid, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchDeleteChoice(List<int> choices, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateChoice(Choice1 choice1, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchUpdateChoice(List<Choice1> choices, CancellationToken cancellationToken = default);
    
    // Choice Two
    public ValueTask<int> AddChoice2(Choice2 choice2, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchInsertChoice2(List<Choice2> choices, CancellationToken cancellationToken = default);
    
    public ValueTask<List<Choice2>> ListChoice2(int choiceid, CancellationToken cancellationToken = default);
    
    public ValueTask<Choice2> FindChoice2(int choice2id, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteChoice2(int choice2id, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchDeleteChoice2(List<int> choices, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateChoice2(Choice2 choice2, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchUpdateChoice2(List<Choice2> choices, CancellationToken cancellationToken = default);
    
    // Option
    public ValueTask<int> AddOption(Option option, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchInsertOption(List<Option> options, CancellationToken cancellationToken = default);
    
    public ValueTask<List<Option>> ListOption(int productid, CancellationToken cancellationToken = default);
    
    public ValueTask<Option> FindOption(int optionid, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteOption(int optionid, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchDeleteOption(List<int> optionids, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateOption(Option option, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchUpdateOption(List<Option> options, CancellationToken cancellationToken = default);
    
    // Option Cat
    public ValueTask<int> AddOptionCat(OptionCat optionCat, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchInsertOptionCat(List<OptionCat> optionCats, CancellationToken cancellationToken = default);
    
    public ValueTask<List<OptionCat>> ListOptionCat(int productid, CancellationToken cancellationToken = default);
    
    public ValueTask<OptionCat> FindOptionCat(int optioncatid, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteOptionCat(int optioncatid, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchDeleteOptionCat(List<OptionCat> optionCats, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateOptionCat(OptionCat optionCat, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchUpdateOptionCat(List<OptionCat> optionCats, CancellationToken cancellationToken = default);
    
    // Weighing Product
    public ValueTask<int> AddWeighingProduct(WeighingProduct weighingProduct, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchInsertWeighingProduct(List<WeighingProduct> weighingProduct, CancellationToken cancellationToken = default);
    
    public ValueTask<List<WeighingProduct>> ListWeighingProduct(CancellationToken cancellationToken = default);
    
    public ValueTask<List<WeighingProduct>> GetWeighingProduct(string productname, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteWeighingProduct(int id, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchDeleteWeighingProduct(List<int> id, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateWeighingProduct(WeighingProduct weighingProduct, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchUpdateWeighingProduct(List<WeighingProduct> weighingProducts, CancellationToken cancellationToken = default);
    
    // Product Image
    public ValueTask<Content.McpToolResponse> GetImage(int productid);

    public ValueTask<Content.McpToolResponse> SaveImage(
        int productid,
        string? imageUrl = null,
        string? imageBase64 = null,
        string? svgContent = null);
    
}
