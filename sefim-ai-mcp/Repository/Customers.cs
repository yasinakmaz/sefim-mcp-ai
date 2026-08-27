namespace SefimMcp.Repository;

[McpServerToolType]
public class Customers (
        ISqlService<Customer> customerService
    ) : ICustomer
{
    [McpServerTool]
    [Description("Adds a new customer to the customers in the “Şefim” app.")]
    public async ValueTask<int> AddCustomer(Customer customer, CancellationToken cancellationToken = default)
    {
        var result = await customerService.InsertAndGetIdAsync<int>(customer, cancellationToken);

        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Adds new customers in bulk to the customers in the “Şefim” app.")]
    public async ValueTask<int> BatchAddCustomer(List<Customer> customers, CancellationToken cancellationToken = default)
    {
        var result = await customerService.BatchInsertAsync(customers, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Deletes the relevant customer from the “Şefim” app. Be sure to inform the user that this action cannot be undone.")]
    public async ValueTask<bool> DeleteCustomer(int customerId, CancellationToken cancellationToken = default)
    {
        var result = await customerService.DeleteAsync(customerId, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("It deactivates the relevant customer in the “Şefim” app.")]
    public async ValueTask<bool> SoftDeleteCustomer(int customerId, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?>()
        {
            ["P1"] = customerId
        };
        
        var result = await customerService.ExecuteNonQueryAsync(Querys.SoftDeleteCustomerQuery, parameters, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes the relevant customers from the “Şefim” app in bulk. Be sure to inform the user that this action cannot be undone.")]
    public async ValueTask<int> BatchDeleteCustomer(List<Customer> customers, CancellationToken cancellationToken = default)
    {
        var result = await customerService.BatchDeleteAsync(customers, 1000, cancellationToken);
        
        return result;
    }

    [McpServerTool]
    [Description("Updates the relevant customer's information in the “Şefim” app.")]
    public async ValueTask<bool> UpdateCustomer(Customer customer, CancellationToken cancellationToken = default)
    {
        var result = await customerService.UpdateAsync(customer, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("Bulk-updates the relevant customer's information in the “Şefim” app.")]
    public async ValueTask<int> BatchUpdateCustomer(List<Customer> customers, CancellationToken cancellationToken = default)
    {
        var result = await customerService.BatchUpdateAsync(customers, 1000, cancellationToken);
        
        return result;
    }

    [McpServerTool]
    [Description("Retrieves the specific customer's information from the “Şefim” app.")]
    public async ValueTask<Customer> GetCustomer(int customerId, CancellationToken cancellationToken = default)
    {
        var result = await customerService.GetByIdAsync(customerId, cancellationToken);
        
        return result ?? new Customer();
    }

    [McpServerTool]
    [Description("It retrieves all customers from the “Şefim” app.")]
    public async ValueTask<List<Customer>> GetCustomers(CancellationToken cancellationToken = default)
    {
        var result = await customerService.GetAllAsync(cancellationToken);

        return result.ToList() ?? new List<Customer>();
    }

    [McpServerTool]
    [Description("It retrieves customers by filtering them on the “Şefim” app.")]
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