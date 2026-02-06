namespace DoliMiddlewareApi.Dtos.Dolibarr;

public class InvoicePaymentResponse
{
    public required string amount { get; set; }
    public string type { get; set; }
    public required string date { get; set; }
    public string num { get; set; }
    public string @ref { get; set; }
}