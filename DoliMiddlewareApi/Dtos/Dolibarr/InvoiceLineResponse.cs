namespace DoliMiddlewareApi.Dtos.Dolibarr;

public class InvoiceLineResponse
{
    public string? id { get; set; }
    public string? desc { get; set; }
    public string? description { get; set; }
    public string? qty { get; set; }
    public string? subprice { get; set; }
    public string? tva_tx { get; set; }
    public string? total_ttc { get; set; }
}