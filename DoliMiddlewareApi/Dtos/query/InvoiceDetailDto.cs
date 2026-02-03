namespace DoliMiddlewareApi.Dtos;

public class InvoiceDetailDto
{
    public int Id { get; set; }
    public string Number { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? ExpireDate { get; set; }
    public int ClientId { get; set; }
    public decimal? Total { get; set; }
    public decimal? RemainToPay { get; set; }
    public string Status { get; set; }
    public List<InvoiceLineDto>? Lines { get; set; }
    public string? note_public { get; set; }
    public string? note_private { get; set; }
}