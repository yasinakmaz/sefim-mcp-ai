namespace SefimMcp.Models;

public sealed class SelectQueryResult
{
    public List<string> Columns { get; set; } = [];

    public List<Dictionary<string, object?>> Rows { get; set; } = [];

    public int RowCount => Rows.Count;
}
