namespace DoliMiddlewareApi.Dtos.query;

public class InvoicePaymentDto
{
    public required string @Ref { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Type { get; set; }
    public string? TransactionNum { get; set; }
    public int Amount { get; set; }
}