namespace SefimMcp.Interfaces;

public interface ITableGroup
{
    public ValueTask<int> AddTableGroup(TableGroup tableGroup, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkInsertTableGroups(List<TableGroup> tableGroups, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateTableGroup(TableGroup tableGroup, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkUpdateTableGroups(List<TableGroup> tableGroups, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteTableGroup(TableGroup tableGroup, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BulkDeleteTableGroups(List<int> tableGroups, CancellationToken cancellationToken = default);
    
    public ValueTask<List<TableGroup>> GetTableGroups(CancellationToken cancellationToken = default);
    
    public ValueTask<TableGroup> GetTableGroup(int id, CancellationToken cancellationToken = default);
}