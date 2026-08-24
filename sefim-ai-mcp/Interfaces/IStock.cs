namespace SefimMcp.Interfaces;

public interface IStock
{
    public ValueTask<int> AddProduct(Product product, CancellationToken cancellationToken = default);
    
    public ValueTask<int> AddChoice(Choice1 choice1, CancellationToken cancellationToken = default);
    
    public ValueTask<int> AddChoice2(Choice2 choice2, CancellationToken cancellationToken = default);
    
    public ValueTask<int> AddOption(Option option, CancellationToken cancellationToken = default);
    
    public ValueTask<int> AddOptionCat(OptionCat optionCat, CancellationToken cancellationToken = default);
    
    public ValueTask<List<Product>> ListProduct(string search, CancellationToken cancellationToken = default);
    
    public ValueTask<List<Choice1>> ListChoice(int ProductId, CancellationToken cancellationToken = default);
}