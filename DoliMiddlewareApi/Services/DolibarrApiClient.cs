using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using DoliMiddlewareApi.Dtos;
using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Exceptions;

namespace DoliMiddlewareApi.Services;

public class DolibarrApiClient
{
    private readonly HttpClient _httpClient;

    public DolibarrApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ===== FACTURAS =====
    public async Task<InvoiceDetailDto> GetInvoiceAsync(int id)
    {
        var data = await GetAsync<InvoiceDetailResponse>($"invoices/{id}");
        return MapToInvoiceDetailDto(data);
    }
    
    public async Task<List<InvoiceDto>> GetInvoicesAsync(
        int limit = 50,
        int page = 1,
        string? status = null)
    {
        // empieza por 1 para el frontend
        var endpoint = $"invoices?limit={limit}&page={page - 1}";

        if (!string.IsNullOrEmpty(status))
        {
            endpoint += $"&status={status}";
        }
        
        var dataList = await GetListAsync<InvoiceResponse>(endpoint);
        return dataList.Select(MapToInvoiceDto).ToList();
    }

    // ===== MAPPER =====
    private InvoiceDto MapToInvoiceDto(InvoiceResponse invoiceResponse)
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

            Status = ConvertStatusToWord(invoiceResponse.statut)
        };
    }
    private InvoiceDetailDto MapToInvoiceDetailDto(InvoiceDetailResponse data)
    {
        // Mapear campos base de la factura
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
            
            Lines = data.Lines?.Select(MapToInvoiceLineDto).ToList() ?? new List<InvoiceLineDto>()
        };
    }

    private InvoiceLineDto MapToInvoiceLineDto(InvoiceLineResponse lineResponse)
    {
        return new InvoiceLineDto
        {
            Id = int.TryParse(lineResponse.id, out int id) ? id : 0,
            Description = lineResponse.description ?? lineResponse.desc ?? "",
            Quantity = decimal.TryParse(lineResponse.qty, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qty)
                ? qty : 0,
            UnitPrice = decimal.TryParse(lineResponse.subprice, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price)
                ? Math.Round(price, 2) : 0,
            TaxRate = decimal.TryParse(lineResponse.tva_tx, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal tax)
                ? Math.Round(tax, 2) : 0,
            Total = decimal.TryParse(lineResponse.total_ttc, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal total)
                ? Math.Round(total, 2) : 0
        };
    }
    

    private static string ConvertStatusToWord(string? statusNumber)
    {
        return statusNumber switch
        {
            "0" => "draft",
            "1" => "unpaid",
            "2" => "paid",
            "3" => "cancelled",
            _ => "unknown"
        };
    }
    // ===== MÉTODOS GENÉRICOS =====
    private async Task<T> GetAsync<T>(string endpoint) where T : class
    {
        var response = await _httpClient.GetAsync(endpoint);
        await HandleErrorsAsync(response, endpoint);

        return await response.Content.ReadFromJsonAsync<T>()
               ?? throw new BadRequestException("Failed to deserialize response");
    }

    private async Task<List<T>> GetListAsync<T>(string endpoint) where T : class
    {
        var response = await _httpClient.GetAsync(endpoint);
        await HandleErrorsAsync(response, endpoint);

        return await response.Content.ReadFromJsonAsync<List<T>>() ?? new List<T>();
    }

    private async Task HandleErrorsAsync(HttpResponseMessage response, string endpoint)
    {
        if (response.IsSuccessStatusCode) return;

        var errorContent = await response.Content.ReadAsStringAsync();

        throw response.StatusCode switch
        {
            HttpStatusCode.NotFound => new NotFoundException($"Resource not found: {endpoint}"),
            HttpStatusCode.Unauthorized => new UnauthorizedException("Invalid API credentials"),
            HttpStatusCode.Forbidden => new ForbiddenException("Access to this resource is forbidden"),
            HttpStatusCode.BadRequest => new BadRequestException($"Bad request: {errorContent}"),
            HttpStatusCode.InternalServerError => new ApiException($"Server error: {errorContent}"),
            _ => new ApiException($"Unexpected error ({response.StatusCode}): {errorContent}")
        };
    }
}