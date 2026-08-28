namespace SefimMcp.Models;

public sealed class PersonnelCollectionReport
{
    public List<Payment> Payments { get; set; } = [];

    public EndOfTheDaySummary Summary { get; set; } = new();
}
