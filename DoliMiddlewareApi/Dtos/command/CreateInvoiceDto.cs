using System.ComponentModel.DataAnnotations;

namespace DoliMiddlewareApi.Dtos.command;

public class CreateInvoiceDto
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public DateTime? ExpireDate { get; set; }

    [StringLength(100)]
    public string? Reference { get; set; }

    public string? NotePublic { get; set; }
    public string? NotePrivate { get; set; }

    public string Status { get; set; } = "draft";

    [Required]
    [MinLength(1)]
    public List<CreateInvoiceLineDto> Lines { get; set; } = new();
}

