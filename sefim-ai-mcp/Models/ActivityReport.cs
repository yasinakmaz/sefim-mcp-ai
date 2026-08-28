namespace SefimMcp.Models;

public sealed class ActivityReport
{
    public List<Payment> Payments { get; set; } = [];

    public EndOfTheDaySummary Summary { get; set; } = new();
}
