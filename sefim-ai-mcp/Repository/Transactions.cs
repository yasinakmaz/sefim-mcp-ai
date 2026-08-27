namespace SefimMcp.Repository;

[McpServerToolType]
public class Transactions (
        ISqlService<DirectTransaction> directTransactionService
    ) : ITransactions
{
    #region DirectTransactions

    [McpServerTool]
    [Description("Performs a cash deposit transaction into the cash register in the “Şefim” app.")]
    public async ValueTask<int> AddDirectTransaction(DirectTransaction transaction, CancellationToken cancellationToken = default)
    {
        var result = await directTransactionService.InsertAndGetIdAsync<int>(transaction, cancellationToken);
        
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("It makes a bulk cash deposit into the cash register in the “Şefim” app.")]
    public async ValueTask<int> BulkInsertDirectTransactions(List<DirectTransaction> transactions, CancellationToken cancellationToken = default)
    {
        var result = await directTransactionService.BatchInsertAsync(transactions, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Updates the relevant cash receipt in the “Şefim” app's cash register.")]
    public async ValueTask<bool> UpdateDirectTransaction(DirectTransaction transaction, CancellationToken cancellationToken = default)
    {
        var result = await directTransactionService.UpdateAsync(transaction, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("It updates cash deposits in the ‘Şefim’ app's cash register in bulk.")]
    public async ValueTask<int> BulkUpdateDirectTransactions(List<DirectTransaction> transactions, CancellationToken cancellationToken = default)
    {
        var result = await directTransactionService.BatchUpdateAsync(transactions, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Deletes the corresponding cash receipt from the cash register in the “Şefim” app.")]
    public async ValueTask<bool> DeleteDirectTransaction(DirectTransaction transaction, CancellationToken cancellationToken = default)
    {
        var result = await directTransactionService.DeleteAsync(transaction, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes the relevant cash receipts in the cash register of the “Şefim” app in bulk.")]
    public async ValueTask<int> BulkDeleteDirectTransactions(List<int> transactions, CancellationToken cancellationToken = default)
    {
        var result = await directTransactionService.BatchDeleteAsync(transactions.Cast<object>(), 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Displays a list of the relevant cash receipts in the “Şefim” app's cash register, sorted by Start and End Date.")]
    public async ValueTask<List<DirectTransaction>> GetDirectTransactions(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        const string whereQuery = "Date >= @startTime AND Date <= @endTime";

        var parameters = new Dictionary<string, object?>()
        {
            ["startTime"] = startTime,
            ["endTime"] = endTime
        };
        
        var result = await directTransactionService.GetWhereAsync(whereQuery, parameters, cancellationToken);

        return result.ToList() ?? new List<DirectTransaction>();
    }

    [McpServerTool]
    [Description("Retrieves the corresponding cash receipt from the cash register in the “Şefim” app.")]
    public async ValueTask<DirectTransaction> GetDirectTransaction(int id, CancellationToken cancellationToken = default)
    {
        var result = await directTransactionService.GetByIdAsync(id, cancellationToken);

        return result ?? new DirectTransaction();
    }

    #endregion
}