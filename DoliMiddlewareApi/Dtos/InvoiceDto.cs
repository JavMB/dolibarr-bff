using DoliMiddlewareApi.Dtos.Enums;

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
    public InvoiceStatus Status { get; set; }
  
    
    
}