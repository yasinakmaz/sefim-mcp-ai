namespace SefimMcp.Interfaces;

public interface ITransactions
{
    // Direct Transaction
    public ValueTask<int> AddDirectTransaction(DirectTransaction transaction, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkInsertDirectTransactions(List<DirectTransaction> transactions, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateDirectTransaction(DirectTransaction transaction, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkUpdateDirectTransactions(List<DirectTransaction> transactions, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteDirectTransaction(DirectTransaction transaction, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkDeleteDirectTransactions(List<int> transactions, CancellationToken cancellationToken = default);
    
    public ValueTask<List<DirectTransaction>> GetDirectTransactions(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
    
    public ValueTask<DirectTransaction> GetDirectTransaction(int id, CancellationToken cancellationToken = default);
}