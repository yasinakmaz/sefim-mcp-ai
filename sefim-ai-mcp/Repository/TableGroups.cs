namespace SefimMcp.Repository;

[McpServerToolType]
public class TableGroups (
        ISqlService<TableGroup> tableGroupService
    ) : ITableGroup
{
    [McpServerTool]
    [Description("It adds items to the tables in the ‘Şefim’ app.")]
    public async ValueTask<int> AddTableGroup(TableGroup tableGroup, CancellationToken cancellationToken = default)
    {
        var result = await tableGroupService.InsertAndGetIdAsync<int>(tableGroup, cancellationToken);
        
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Adds tables in bulk to the ‘Şefim’ app.")]
    public async ValueTask<int> BulkInsertTableGroups(List<TableGroup> tableGroups, CancellationToken cancellationToken = default)
    {
        var result = await tableGroupService.BatchInsertAsync(tableGroups, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("It updates the relevant table's bill in the “Şefim” app.")]
    public async ValueTask<bool> UpdateTableGroup(TableGroup tableGroup, CancellationToken cancellationToken = default)
    {
        var result = await tableGroupService.UpdateAsync(tableGroup, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("It updates the relevant table's bill in the ‘Şefim’ app all at once.")]
    public async ValueTask<int> BulkUpdateTableGroups(List<TableGroup> tableGroups, CancellationToken cancellationToken = default)
    {
        var result = await tableGroupService.BatchUpdateAsync(tableGroups, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("It deletes the corresponding table from the ‘Şefim’ app.")]
    public async ValueTask<bool> DeleteTableGroup(TableGroup tableGroup, CancellationToken cancellationToken = default)
    {
        var result = await tableGroupService.DeleteAsync(tableGroup, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes the relevant tables associated with the ‘Şefim’ app in bulk.")]
    public async ValueTask<int> BulkDeleteTableGroups(List<int> tableGroups, CancellationToken cancellationToken = default)
    {
        var result = await tableGroupService.BatchDeleteAsync(tableGroups.Cast<object>(), 1000, cancellationToken);
        
        return result;
    }

    [McpServerTool]
    [Description("Displays a list of tables with bills in the “Şefim” app.")]
    public async ValueTask<List<TableGroup>> GetTableGroups(CancellationToken cancellationToken = default)
    {
        var result = await tableGroupService.GetAllAsync(cancellationToken);

        return result.ToList() ?? new List<TableGroup>();
    }

    [McpServerTool]
    [Description("The “Şefim” app retrieves the relevant table's check.")]
    public async ValueTask<TableGroup> GetTableGroup(int id, CancellationToken cancellationToken = default)
    {
        var result = await tableGroupService.GetByIdAsync(id, cancellationToken);
        
        return result ?? new TableGroup();
    }
}