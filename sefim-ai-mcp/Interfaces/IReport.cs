namespace SefimMcp.Interfaces;

public interface IReport
{
    public ValueTask<EndOfTheDayReport> EndOfTheDay(DateTime start, DateTime end, CancellationToken cancellationToken = default);

    public ValueTask<List<DailyTotalsReport>> DailyTotals(DateTime start, DateTime end, CancellationToken cancellationToken = default);

    public ValueTask<RevenueSummaryReport> RevenueSummary(DateTime start, DateTime end, CancellationToken cancellationToken = default);

    public ValueTask<TableRevenueReport> TableRevenue(DateTime start, DateTime end, CancellationToken cancellationToken = default);

    public ValueTask<PersonnelCollectionReport> PersonnelCollection(DateTime start, DateTime end, CancellationToken cancellationToken = default);

    public ValueTask<ActivityReport> Activity(DateTime start, DateTime end, CancellationToken cancellationToken = default);

    public ValueTask<List<DirectTransaction>> IncomeExpenseRecords(DateTime start, DateTime end, CancellationToken cancellationToken = default);

    public ValueTask<List<DeletedBill>> DeletedOrders(DateTime start, DateTime end, CancellationToken cancellationToken = default);

    public ValueTask<List<SalesReportItem>> SalesReport(DateTime start, DateTime end, int billType1 = 0, int billType2 = 1, int billType3 = 2, CancellationToken cancellationToken = default);

    public ValueTask<List<SalesTotalsReport>> SalesTotals(DateTime start, DateTime end, int billType1 = 0, int billType2 = 1, int billType3 = 2, CancellationToken cancellationToken = default);

    public ValueTask<SelectQueryResult> AiSelectQuery(string query, CancellationToken cancellationToken = default);
}
