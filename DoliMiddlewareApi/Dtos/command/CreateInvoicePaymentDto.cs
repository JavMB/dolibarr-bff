using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DoliMiddlewareApi.Dtos.command;

public class CreateInvoicePaymentDto
{
    // Si se especifica, es un pago parcial. Si es null, paga todo lo pendiente.
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal? Amount { get; set; }

    [Required] public DateTime PaymentDate { get; set; }

    // Acepta ambos nombres: paymentMethodId (del frontend) o paymentModeId (fallback)
    [JsonPropertyName("paymentMethodId")]
    public int? PaymentMethodId { get; set; }

    [JsonPropertyName("paymentModeId")]
    public int PaymentModeId { get; set; }

    [Required] [RegularExpression("yes|no", ErrorMessage = "Must be 'yes' or 'no'")]
    public string ClosePaidInvoices { get; set; } = "yes";

    public int AccountId { get; set; } = 1;

    [JsonPropertyName("numPayment")]
    public string? PaymentNumber { get; set; }
} 