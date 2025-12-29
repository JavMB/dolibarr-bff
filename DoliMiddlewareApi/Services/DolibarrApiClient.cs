using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using DoliMiddlewareApi.Dtos;
using DoliMiddlewareApi.Dtos.Enums;
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
    public async Task<InvoiceDto> GetInvoiceAsync(int id)
    {
        var data = await GetAsync<InvoiceResponse>($"invoices/{id}");
        return MapToInvoiceDto(data);
    }

    public async Task<List<InvoiceDto>> GetInvoicesAsync()
    {
        var dataList = await GetListAsync<InvoiceResponse>("invoices");
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

            Total = decimal.TryParse(invoiceResponse.total_ttc, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal total)
                ? Math.Round(total, 2)
                : null,
            RemainToPay = decimal.TryParse(invoiceResponse.remaintopay, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal remain)
                ? Math.Round(remain, 2)
                : null,

            Status = InvoiceStatusMapper.FromDolibarrStatus(invoiceResponse.statut ?? "0")
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