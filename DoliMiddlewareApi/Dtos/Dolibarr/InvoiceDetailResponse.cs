namespace DoliMiddlewareApi.Dtos.Dolibarr;

public class InvoiceDetailResponse : InvoiceResponse
{
    public List<InvoiceLineResponse>? Lines { get; set; }
    
}