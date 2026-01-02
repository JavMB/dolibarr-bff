using System.ComponentModel.DataAnnotations;

namespace DoliMiddlewareApi.Dtos.command;

public class UpdateInvoiceDto
{
    // ClientId y Date no se cambian en PUT (Dolibarr no permite)

    public DateTime? ExpireDate { get; set; }

    [StringLength(100)]
    public string? Number { get; set; }  // Cambiado de Reference a Number para consistencia con GET

    public string? NotePublic { get; set; }
    public string? NotePrivate { get; set; }

    public string Status { get; set; } = "draft";

    // Lines quitadas - usa POST /lines para añadir líneas
}