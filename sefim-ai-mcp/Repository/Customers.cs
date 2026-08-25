namespace SefimMcp.Repository;

public class Customers (
        ISqlService<Customer> customerService
    ) : ICustomer
{
    [McpServerTool]
    [Description("")]
    public async ValueTask<int> AddCustomer(Customer customer, CancellationToken cancellationToken = default)
    {
        var result = await customerService.InsertAndGetIdAsync<int>(customer, cancellationToken);

        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("")]
    public async ValueTask<int> BatchAddCustomer(List<Customer> customers, CancellationToken cancellationToken = default)
    {
        var result = await customerService.BatchInsertAsync(customers, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("")]
    public async ValueTask<bool> DeleteCustomer(int customerId, CancellationToken cancellationToken = default)
    {
        var result = await customerService.DeleteAsync(customerId, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("")]
    public async ValueTask<int> BatchDeleteCustomer(List<Customer> customers, CancellationToken cancellationToken = default)
    {
        var result = await customerService.BatchDeleteAsync(customers, 1000, cancellationToken);
        
        return result;
    }

    [McpServerTool]
    [Description("")]
    public async ValueTask<bool> UpdateCustomer(Customer customer, CancellationToken cancellationToken = default)
    {
        var result = await customerService.UpdateAsync(customer, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("")]
    public async ValueTask<int> BatchUpdateCustomer(List<Customer> customers, CancellationToken cancellationToken = default)
    {
        var result = await customerService.BatchUpdateAsync(customers, 1000, cancellationToken);
        
        return result;
    }

    [McpServerTool]
    [Description("")]
    public async ValueTask<Customer> GetCustomer(int customerId, CancellationToken cancellationToken = default)
    {
        var result = await customerService.GetByIdAsync(customerId, cancellationToken);
        
        return result ?? new Customer();
    }

    [McpServerTool]
    [Description("")]
    public async ValueTask<List<Customer>> GetCustomers(CancellationToken cancellationToken = default)
    {
        var result = await customerService.GetAllAsync(cancellationToken);

        return result.ToList() ?? new List<Customer>();
    }

    [McpServerTool]
    [Description("")]
    public async ValueTask<List<Customer>> SearchCustomers(string search, CancellationToken cancellationToken = default)
    {
        string normalize_search = $"%{search}%";
        
        var parameters = new Dictionary<string, object?>()
        {
            ["Search"] = normalize_search
        };

        var result = await customerService.ExecuteRawQueryAsync(Querys.CustomerListQuery, parameters, cancellationToken);
        
        return result.ToList() ?? new List<Customer>();
    }
    
    
}