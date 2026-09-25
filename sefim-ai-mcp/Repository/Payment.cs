namespace SefimMcp.Repository;

public class Payment (
        ISqlService<BillWithHeader> billService,
        ISqlService<Models.Payment> paymentService,
        ISqlService<PaymentDetail> paymentDetailService,
        ISqlService<PrePayment> prePaymentService,
        ISqlService<DebitPayment> debitPaymentService,
        ISqlService<PaymentPaket> paymentPaketService
    ) : IPayment
{
    [McpServerTool]
    [Description("BillWithHeader ")]
    public async ValueTask<List<BillWithHeader>> GetBillWithHeader(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        const string whereQuery = "Date >= @startDate AND Date <= @endDate";

        var parameters = new Dictionary<string, object?>()
        {
            ["startDate"] = startDate, 
            ["endDate"] = endDate
        };

        var result = await billService.ExecuteRawQueryAsync(whereQuery, parameters, cancellationToken);

        return result.ToList();
    }

    [McpServerTool]
    [Description("Payment ")]
    public async ValueTask<List<Models.Payment>> GetPayments(int headerId, CancellationToken cancellationToken = default)
    {
        const string whereQuery = "HeaderId = @headerId";

        var parameters = new Dictionary<string, object?>()
        {
            ["headerId"] = headerId
        };

        var result = await paymentService.ExecuteRawQueryAsync(whereQuery, parameters, cancellationToken);

        return result.ToList();
    }

    [McpServerTool]
    [Description("Payment Details")]
    public async ValueTask<List<PaymentDetail>> GetPaymentDetails(int paymentId, CancellationToken cancellationToken = default)
    {
        const string whereQuery = "PaymentId = @PaymentId";

        var parameters = new Dictionary<string, object?>()
        {
            ["PaymentId"] = paymentId
        };

        var result = await paymentDetailService.ExecuteRawQueryAsync(whereQuery, parameters, cancellationToken);

        return result.ToList();
    }

    [McpServerTool]
    [Description("Pre Payments")]
    public async ValueTask<List<PrePayment>> GetPrePayments(string tableNumber, CancellationToken cancellationToken = default)
    {
        const string whereQuery = "TableNumber = @TableNumber";

        var parameters = new Dictionary<string, object?>()
        {
            ["TableNumber"] = tableNumber
        };

        var result = await prePaymentService.ExecuteRawQueryAsync(whereQuery, parameters, cancellationToken);

        return result.ToList();
    }

    [McpServerTool]
    [Description("Debit Payments")]
    public async ValueTask<List<DebitPayment>> GetDebitPayments(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        const string whereQuery = "Date >= @startDate AND Date <= @endDate";

        var parameters = new Dictionary<string, object?>()
        {
            ["startDate"] = startDate,
            ["endDate"] = endDate
        };

        var result = await debitPaymentService.ExecuteRawQueryAsync(whereQuery, parameters, cancellationToken);

        return result.ToList();
    }

    [McpServerTool]
    [Description("Payment Pakets")]
    public async ValueTask<List<PaymentPaket>> GetPaymentPakets(int headerId, CancellationToken cancellationToken = default)
    {
        const string whereQuery = "HeaderId = @HeaderId";

        var parameters = new Dictionary<string, object?>()
        {
            ["HeaderId"] = headerId
        };

        var result = await paymentPaketService.ExecuteRawQueryAsync(whereQuery, parameters, cancellationToken);
        
        return result.ToList();
    }
}