using System.ComponentModel.DataAnnotations;

namespace DoliMiddlewareApi.Dtos.command;

public class UpdateInvoiceStatusDto
{
    [Required]
    [RegularExpression("^(draft|unpaid|paid)$")]
    public string Status { get; set; } = "draft";
}
