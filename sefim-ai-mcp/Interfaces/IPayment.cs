namespace SefimMcp.Interfaces;

public interface IPayment
{
    public ValueTask<List<BillWithHeader>> GetBillWithHeader(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    public ValueTask<List<Models.Payment>> GetPayments(int headerId, CancellationToken cancellationToken = default);
    
    public ValueTask<List<PaymentDetail>> GetPaymentDetails(int paymentId, CancellationToken cancellationToken = default);
    
    public ValueTask<List<PrePayment>> GetPrePayments(string tableNumber, CancellationToken cancellationToken = default);
    
    public ValueTask<List<DebitPayment>> GetDebitPayments(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    public ValueTask<List<PaymentPaket>> GetPaymentPakets(int headerId, CancellationToken cancellationToken = default);
}