using System.Net;
using DoliMiddlewareApi.Exceptions;

namespace DoliMiddlewareApi.Services.Clients;

public class DolibarrApiClient
{
    private readonly HttpClient _httpClient;

    public DolibarrApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    
    // ===== MÉTODOS GENÉRICOS =====
    public async Task<T> GetResourceAsync<T>(string endpoint) where T : class
    {
        var response = await _httpClient.GetAsync(endpoint);
        await EnsureSuccessOrThrowAsync(response, endpoint);

        return await response.Content.ReadFromJsonAsync<T>()
               ?? throw new ApiException($"Failed to deserialize response from Dolibarr for endpoint '{endpoint}'");
    }

    public async Task<List<T>> GetCollectionAsync<T>(string endpoint) where T : class
    {
        var response = await _httpClient.GetAsync(endpoint);
        await EnsureSuccessOrThrowAsync(response, endpoint);

        return await response.Content.ReadFromJsonAsync<List<T>>()
               ?? throw new ApiException(
                   $"Failed to deserialize list response from Dolibarr for endpoint '{endpoint}'");
    }

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    private async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, string endpoint)
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