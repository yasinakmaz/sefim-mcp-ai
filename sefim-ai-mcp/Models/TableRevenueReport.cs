namespace SefimMcp.Models;

public sealed class TableRevenueReport
{
    public List<Payment> Payments { get; set; } = [];

    public EndOfTheDaySummary Summary { get; set; } = new();
}
