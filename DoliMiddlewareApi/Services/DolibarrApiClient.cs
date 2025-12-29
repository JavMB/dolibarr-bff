using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using DoliMiddlewareApi.Dtos;
using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Exceptions;
using DoliMiddlewareApi.Mappers;

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
        return InvoiceMapper.MapToInvoiceDetailDto(data);
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
        return dataList.Select(InvoiceMapper.MapToInvoiceDto).ToList();
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