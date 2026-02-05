namespace DoliMiddlewareApi.Dtos;

public class InvoiceDto
{
    public int Id { get; set; }
    public string Number { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? ExpireDate { get; set; }
    public int ClientId { get; set; }
    public decimal? Total { get; set; }
    public decimal? RemainToPay { get; set; }
    public string Status { get; set; }
    public string? NotePublic { get; set; }
    public string? NotePrivate { get; set; }
    public string? ClientName { get; set; }
}