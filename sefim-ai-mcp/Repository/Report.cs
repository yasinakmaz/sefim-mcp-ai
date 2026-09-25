using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace SefimMcp.Repository;

public class Report(
    IConnectionFactory connectionFactory,
    IEntityMapper entityMapper
    ) : IReport
{
    public ValueTask<SelectQueryResult> AiSelectQuery(string query, CancellationToken cancellationToken = default)
        => AiSelectQuery(query, 100, cancellationToken);

    [McpServerTool]
    [Description("Returns the complete end-of-the-day report for the selected date range in one SQL round-trip.")]
    public async ValueTask<EndOfTheDayReport> EndOfTheDay(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(end));

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();

        command.CommandText = Querys.EndOfTheDayQuery;
        command.CommandTimeout = connectionFactory.GetCommandTimeout();
        command.Parameters.Add("@par1", SqlDbType.DateTime).Value = start;
        command.Parameters.Add("@par2", SqlDbType.DateTime).Value = end;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var report = new EndOfTheDayReport
        {
            Receivables = await ReadResultSetAsync<EndOfTheDayReceivable>(reader, cancellationToken)
        };

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.DeletedBills = await ReadResultSetAsync<DeletedBill>(reader, cancellationToken);

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.Summary = await ReadSingleResultSetAsync<EndOfTheDaySummary>(reader, cancellationToken) ?? new EndOfTheDaySummary();

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.UserSales = await ReadResultSetAsync<EndOfTheDayUserSale>(reader, cancellationToken);

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.ProductSales = await ReadResultSetAsync<EndOfTheDayProductSale>(reader, cancellationToken);

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.FreeItems = await ReadResultSetAsync<PaidBill>(reader, cancellationToken);

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.DebitPayments = await ReadResultSetAsync<Models.Payment>(reader, cancellationToken);

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.DiscountedPayments = await ReadResultSetAsync<Models.Payment>(reader, cancellationToken);

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.DirectTransactions = await ReadResultSetAsync<DirectTransaction>(reader, cancellationToken);

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.ReturnItems = await ReadResultSetAsync<EndOfTheDayReturnItem>(reader, cancellationToken);

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.WasteItems = await ReadResultSetAsync<PaidBill>(reader, cancellationToken);

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.PrePaymentSummary = await ReadSingleResultSetAsync<EndOfTheDayPrePaymentSummary>(reader, cancellationToken) ?? new EndOfTheDayPrePaymentSummary();

        return report;
    }

    [McpServerTool]
    [Description("Returns daily payment and transaction totals for the selected date range.")]
    public async ValueTask<List<DailyTotalsReport>> DailyTotals(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(end));

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();

        command.CommandText = Querys.DailyTotalsQuery;
        command.CommandTimeout = connectionFactory.GetCommandTimeout();
        command.Parameters.Add("@par1", SqlDbType.DateTime).Value = start;
        command.Parameters.Add("@par2", SqlDbType.DateTime).Value = end;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await ReadResultSetAsync<DailyTotalsReport>(reader, cancellationToken);
    }

    [McpServerTool]
    [Description("Returns the revenue summary report and payment rows for the selected date range.")]
    public async ValueTask<RevenueSummaryReport> RevenueSummary(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(end));

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();

        command.CommandText = Querys.RevenueSummaryQuery;
        command.CommandTimeout = connectionFactory.GetCommandTimeout();
        command.Parameters.Add("@par1", SqlDbType.DateTime).Value = start;
        command.Parameters.Add("@par2", SqlDbType.DateTime).Value = end;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var report = new RevenueSummaryReport
        {
            Payments = await ReadResultSetAsync<Models.Payment>(reader, cancellationToken)
        };

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.Summary = await ReadSingleResultSetAsync<EndOfTheDaySummary>(reader, cancellationToken) ?? new EndOfTheDaySummary();

        return report;
    }

    [McpServerTool]
    [Description("Returns the table revenue report and summary totals for the selected date range.")]
    public async ValueTask<TableRevenueReport> TableRevenue(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(end));

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();

        command.CommandText = Querys.TableRevenueQuery;
        command.CommandTimeout = connectionFactory.GetCommandTimeout();
        command.Parameters.Add("@par1", SqlDbType.DateTime).Value = start;
        command.Parameters.Add("@par2", SqlDbType.DateTime).Value = end;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var report = new TableRevenueReport
        {
            Payments = await ReadResultSetAsync<Models.Payment>(reader, cancellationToken)
        };

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.Summary = await ReadSingleResultSetAsync<EndOfTheDaySummary>(reader, cancellationToken) ?? new EndOfTheDaySummary();

        return report;
    }

    [McpServerTool]
    [Description("Returns the personnel collection report and summary totals for the selected date range.")]
    public async ValueTask<PersonnelCollectionReport> PersonnelCollection(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(end));

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();

        command.CommandText = Querys.PersonnelCollectionQuery;
        command.CommandTimeout = connectionFactory.GetCommandTimeout();
        command.Parameters.Add("@par1", SqlDbType.DateTime).Value = start;
        command.Parameters.Add("@par2", SqlDbType.DateTime).Value = end;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var report = new PersonnelCollectionReport
        {
            Payments = await ReadResultSetAsync<Models.Payment>(reader, cancellationToken)
        };

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.Summary = await ReadSingleResultSetAsync<EndOfTheDaySummary>(reader, cancellationToken) ?? new EndOfTheDaySummary();

        return report;
    }

    [McpServerTool]
    [Description("Returns the activity report and summary totals for the selected date range.")]
    public async ValueTask<ActivityReport> Activity(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(end));

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();

        command.CommandText = Querys.ActivityReportQuery;
        command.CommandTimeout = connectionFactory.GetCommandTimeout();
        command.Parameters.Add("@par1", SqlDbType.DateTime).Value = start;
        command.Parameters.Add("@par2", SqlDbType.DateTime).Value = end;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var report = new ActivityReport
        {
            Payments = await ReadResultSetAsync<Models.Payment>(reader, cancellationToken)
        };

        await MoveNextResultSetAsync(reader, cancellationToken);
        report.Summary = await ReadSingleResultSetAsync<EndOfTheDaySummary>(reader, cancellationToken) ?? new EndOfTheDaySummary();

        return report;
    }

    [McpServerTool]
    [Description("Returns income and expense transaction records for the selected date range.")]
    public async ValueTask<List<DirectTransaction>> IncomeExpenseRecords(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(end));

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();

        command.CommandText = Querys.IncomeExpenseRecordsQuery;
        command.CommandTimeout = connectionFactory.GetCommandTimeout();
        command.Parameters.Add("@par1", SqlDbType.DateTime).Value = start;
        command.Parameters.Add("@par2", SqlDbType.DateTime).Value = end;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await ReadResultSetAsync<DirectTransaction>(reader, cancellationToken);
    }

    [McpServerTool]
    [Description("Returns deleted order rows for the selected date range.")]
    public async ValueTask<List<DeletedBill>> DeletedOrders(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(end));

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();

        command.CommandText = Querys.DeletedOrdersQuery;
        command.CommandTimeout = connectionFactory.GetCommandTimeout();
        command.Parameters.Add("@par1", SqlDbType.DateTime).Value = start;
        command.Parameters.Add("@par2", SqlDbType.DateTime).Value = end;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await ReadResultSetAsync<DeletedBill>(reader, cancellationToken);
    }

    [McpServerTool]
    [Description("Returns sales detail rows with net amount for the selected date range and bill types.")]
    public async ValueTask<List<SalesReportItem>> SalesReport(
        DateTime start,
        DateTime end,
        int billType1 = 0,
        int billType2 = 1,
        int billType3 = 2,
        CancellationToken cancellationToken = default)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(end));

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();

        command.CommandText = Querys.SalesReportQuery;
        command.CommandTimeout = connectionFactory.GetCommandTimeout();
        AddDateRangeParameters(command, start, end);
        AddBillTypeParameters(command, billType1, billType2, billType3);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await ReadResultSetAsync<SalesReportItem>(reader, cancellationToken);
    }

    [McpServerTool]
    [Description("Returns sales totals grouped by product for the selected date range and bill types.")]
    public async ValueTask<List<SalesTotalsReport>> SalesTotals(
        DateTime start,
        DateTime end,
        int billType1 = 0,
        int billType2 = 1,
        int billType3 = 2,
        CancellationToken cancellationToken = default)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(end));

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();

        command.CommandText = Querys.SalesTotalsQuery;
        command.CommandTimeout = connectionFactory.GetCommandTimeout();
        AddDateRangeParameters(command, start, end);
        AddBillTypeParameters(command, billType1, billType2, billType3);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await ReadResultSetAsync<SalesTotalsReport>(reader, cancellationToken);
    }

    [McpServerTool]
    [Description("Runs a custom read-only SELECT query for an approved local SQL Server account. Results are capped at 200 rows; this tool never accepts data-modification statements.")]
    public async ValueTask<SelectQueryResult> AiSelectQuery(
        [Description("One SELECT or CTE SELECT statement. Do not use data modification, EXEC, or multiple statements.")] string query,
        [Description("Maximum rows to return, from 1 to 200.")] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var guardFailure = ReadOnlySqlGuard.Validate(query);
        if (guardFailure is not null)
            throw new ArgumentException(guardFailure, nameof(query));

        limit = Math.Clamp(limit, 1, 200);

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();

        command.CommandText = $"SET ROWCOUNT {limit};\n{query}\nSET ROWCOUNT 0;";
        command.CommandTimeout = connectionFactory.GetCommandTimeout();

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken);

        return await ReadDynamicResultSetAsync(reader, cancellationToken);
    }

    private static void AddDateRangeParameters(SqlCommand command, DateTime start, DateTime end)
    {
        command.Parameters.Add("@par1", SqlDbType.DateTime).Value = start;
        command.Parameters.Add("@par2", SqlDbType.DateTime).Value = end;
    }

    private static void AddBillTypeParameters(SqlCommand command, int billType1, int billType2, int billType3)
    {
        command.Parameters.Add("@par3", SqlDbType.Int).Value = billType1;
        command.Parameters.Add("@par4", SqlDbType.Int).Value = billType2;
        command.Parameters.Add("@par5", SqlDbType.Int).Value = billType3;
    }

    private static async Task<SelectQueryResult> ReadDynamicResultSetAsync(SqlDataReader reader, CancellationToken cancellationToken)
    {
        var columns = BuildUniqueColumnNames(reader);
        var rows = new List<Dictionary<string, object?>>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new Dictionary<string, object?>(columns.Count, StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < columns.Count; i++)
            {
                row[columns[i]] = NormalizeDbValue(reader.GetValue(i));
            }

            rows.Add(row);
        }

        return new SelectQueryResult
        {
            Columns = columns,
            Rows = rows
        };
    }

    private static List<string> BuildUniqueColumnNames(SqlDataReader reader)
    {
        var columns = new List<string>(reader.FieldCount);
        var occurrences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);
            if (IsSensitiveColumn(name))
                throw new InvalidOperationException("The query selected a sensitive column that cannot be returned through MCP.");
            if (string.IsNullOrWhiteSpace(name))
                name = $"Column{i + 1}";

            if (occurrences.TryGetValue(name, out var occurrence))
            {
                occurrence++;
                occurrences[name] = occurrence;
                columns.Add($"{name}_{occurrence}");
                continue;
            }

            occurrences[name] = 1;
            columns.Add(name);
        }

        return columns;
    }

    private static bool IsSensitiveColumn(string name)
    {
        return name.Contains("password", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("token", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("connectionstring", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("apikey", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("privatekey", StringComparison.OrdinalIgnoreCase);
    }

    private static object? NormalizeDbValue(object value)
    {
        return value switch
        {
            DBNull => null,
            byte[] bytes => Convert.ToBase64String(bytes),
            TimeSpan timeSpan => timeSpan.ToString(),
            DateOnly dateOnly => dateOnly.ToString("O"),
            TimeOnly timeOnly => timeOnly.ToString("O"),
            _ => value
        };
    }

    private async Task<List<T>> ReadResultSetAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        SqlDataReader reader,
        CancellationToken cancellationToken)
        where T : class
    {
        var rows = new List<T>();

        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(entityMapper.MapFromReader<T>(reader));
        }

        return rows;
    }

    private async Task<T?> ReadSingleResultSetAsync<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        SqlDataReader reader,
        CancellationToken cancellationToken)
        where T : class
    {
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var row = entityMapper.MapFromReader<T>(reader);

        while (await reader.ReadAsync(cancellationToken))
        {
        }

        return row;
    }

    private static async Task MoveNextResultSetAsync(SqlDataReader reader, CancellationToken cancellationToken)
    {
        if (!await reader.NextResultAsync(cancellationToken))
            throw new InvalidOperationException("The end-of-the-day report query returned fewer result sets than expected.");
    }
}
