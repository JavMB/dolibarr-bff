using System.Globalization;
using DoliMiddlewareApi.Dtos;
using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Dtos.query;
using DoliMiddlewareApi.Exceptions;

namespace DoliMiddlewareApi.Mappers;

public static class InvoiceMapper
{
    // ===== MAPPER =====
    public static InvoiceDto MapToInvoiceDto(InvoiceResponse invoiceResponse)
    {
        return new InvoiceDto
        {
            Id = int.TryParse(invoiceResponse.id, out int id) ? id : 0,
            Number = invoiceResponse.@ref ?? "SIN-REF",

            Date = invoiceResponse.date.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(invoiceResponse.date.Value).DateTime
                : null,

            ExpireDate = invoiceResponse.date_lim_reglement.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(invoiceResponse.date_lim_reglement.Value).DateTime
                : null,

            ClientId = int.TryParse(invoiceResponse.socid, out int clientId) ? clientId : 0,

            Total = decimal.TryParse(invoiceResponse.total_ttc, NumberStyles.Any, CultureInfo.InvariantCulture,
                out decimal total)
                ? Math.Round(total, 2)
                : null,
            RemainToPay = decimal.TryParse(invoiceResponse.remaintopay, NumberStyles.Any, CultureInfo.InvariantCulture,
                out decimal remain)
                ? Math.Round(remain, 2)
                : null,

            Status = ConvertStatusToWord(invoiceResponse.statut),
            NotePublic = invoiceResponse.note_public,
            NotePrivate = invoiceResponse.note_private
        };
    }

    public static InvoiceDetailDto MapToInvoiceDetailDto(InvoiceDetailResponse data)
    {
        var baseDto = MapToInvoiceDto(data);

        return new InvoiceDetailDto
        {
            Id = baseDto.Id,
            Number = baseDto.Number,
            Date = baseDto.Date,
            ExpireDate = baseDto.ExpireDate,
            ClientId = baseDto.ClientId,
            Total = baseDto.Total,
            RemainToPay = baseDto.RemainToPay,
            Status = baseDto.Status,
            NotePublic = baseDto.NotePublic,
            NotePrivate = baseDto.NotePrivate,

            Lines = data.Lines?.Select(MapToInvoiceLineDto).ToList() ?? new List<InvoiceLineDto>()
        };
    }

    public static InvoiceLineDto MapToInvoiceLineDto(InvoiceLineResponse lineResponse)
    {
        return new InvoiceLineDto
        {
            Id = int.TryParse(lineResponse.id, out int id) ? id : 0,
            Description = lineResponse.description ?? lineResponse.desc ?? "",
            Quantity = decimal.TryParse(lineResponse.qty, NumberStyles.Any, CultureInfo.InvariantCulture,
                out decimal qty)
                ? qty
                : 0,
            UnitPrice = decimal.TryParse(lineResponse.subprice, NumberStyles.Any, CultureInfo.InvariantCulture,
                out decimal price)
                ? Math.Round(price, 2)
                : 0,
            TaxRate = decimal.TryParse(lineResponse.tva_tx, NumberStyles.Any, CultureInfo.InvariantCulture,
                out decimal tax)
                ? Math.Round(tax, 2)
                : 0,
            Total = decimal.TryParse(lineResponse.total_ttc, NumberStyles.Any, CultureInfo.InvariantCulture,
                out decimal total)
                ? Math.Round(total, 2)
                : 0
        };
    }
    
   

    private static string ConvertStatusToWord(string? statusNumber)
    {
        return statusNumber switch
        {
            "0" => "draft",
            "1" => "unpaid",
            "2" => "paid",
            _ => "unknown"
        };
    }

    public static string ConvertStatusToDolibarr(string status)
    {
        return status.ToLower() switch
        {
            "draft" => "0",
            "unpaid" => "1",
            "paid" => "2",
            _ => throw new BadRequestException($"Estado inválido: {status}. Valores válidos: draft, unpaid, paid")
        };
    }


    public static InvoicePaymentDto MapToInvoicePaymentDto(InvoicePaymentResponse response)
    {
        return new InvoicePaymentDto
        {
            Ref = response.@ref ?? "",
            PaymentDate = DateTime.TryParseExact(response.date, "yyyy-MM-dd HH:mm:ss", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? DateOnly.FromDateTime(date) : DateOnly.MinValue,
            Type = response.type ?? "",
            TransactionNum = response.num ?? "",
            Amount = decimal.TryParse(response.amount, NumberStyles.Any, CultureInfo.InvariantCulture,
                out decimal amount) ? Math.Round(amount, 2) : 0
        };
    }
}