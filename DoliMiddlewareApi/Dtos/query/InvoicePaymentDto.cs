namespace DoliMiddlewareApi.Dtos.query;

public class InvoicePaymentDto
{
    public required string @Ref { get; set; }
    public DateOnly PaymentDate { get; set; }
    public string? Type { get; set; }
    public string? TransactionNum { get; set; }
    public decimal Amount { get; set; }
}