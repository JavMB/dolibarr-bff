using System.ComponentModel.DataAnnotations;

namespace DoliMiddlewareApi.Dtos.command;

public class CreateInvoiceLineDto
{
    [Required]
    [StringLength(500)]  
    public string? Description { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]  
    public decimal Quantity { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]  
    public decimal UnitPrice { get; set; }
    
    [Range(0, 100)]  
    public decimal TaxRate { get; set; } = 0;
    
    
}