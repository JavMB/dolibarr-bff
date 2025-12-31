using DoliMiddlewareApi.Dtos;
using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Mappers;
using DoliMiddlewareApi.Services.Clients;

namespace DoliMiddlewareApi.Services;

public class InvoiceService
{
    private readonly DolibarrApiClient _apiClient;

    public InvoiceService(DolibarrApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<InvoiceDetailDto> GetInvoiceAsync(int id)
    {
        var data = await _apiClient.GetResourceAsync<InvoiceDetailResponse>($"invoices/{id}");
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

        var dataList = await _apiClient.GetCollectionAsync<InvoiceResponse>(endpoint);
        return dataList.Select(InvoiceMapper.MapToInvoiceDto).ToList();
    }
}