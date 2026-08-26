namespace SefimMcp.Interfaces;

public interface ICustomer
{
    public ValueTask<int> AddCustomer(Customer customer, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchAddCustomer(List<Customer> customers, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteCustomer(int customerId, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> SoftDeleteCustomer(int customerId, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchDeleteCustomer(List<Customer> customers, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateCustomer(Customer customer, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchUpdateCustomer(List<Customer> customers, CancellationToken cancellationToken = default);
    
    public ValueTask<Customer> GetCustomer(int customerId, CancellationToken cancellationToken = default);
    
    public ValueTask<List<Customer>> GetCustomers(CancellationToken cancellationToken = default);
    
    public ValueTask<List<Customer>> SearchCustomers(string search, CancellationToken cancellationToken = default);
}