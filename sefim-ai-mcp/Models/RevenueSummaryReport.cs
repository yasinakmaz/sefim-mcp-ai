namespace SefimMcp.Models;

public sealed class RevenueSummaryReport
{
    public List<Payment> Payments { get; set; } = [];

    public EndOfTheDaySummary Summary { get; set; } = new();
}
