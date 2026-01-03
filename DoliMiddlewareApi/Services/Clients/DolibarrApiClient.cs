using System.Net;
using DoliMiddlewareApi.Exceptions;
using DoliMiddlewareApi.Services.Auth;

namespace DoliMiddlewareApi.Services.Clients;

public class DolibarrApiClient(HttpClient httpClient, DolibarrTokenCacheService tokenCacheService)
    : IDolibarrApiClient
{
    public async Task<T> GetResourceAsync<T>(string endpoint) where T : class
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddDolibarrTokenHeader(request);
        var response = await httpClient.SendAsync(request);
        await EnsureSuccessOrThrowAsync(response, endpoint);

        return await response.Content.ReadFromJsonAsync<T>()
               ?? throw new ApiException($"Failed to deserialize response from Dolibarr for endpoint '{endpoint}'");
    }

    public async Task<List<T>> GetCollectionAsync<T>(string endpoint) where T : class
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddDolibarrTokenHeader(request);
        var response = await httpClient.SendAsync(request);
        await EnsureSuccessOrThrowAsync(response, endpoint);

        return await response.Content.ReadFromJsonAsync<List<T>>()
               ?? throw new ApiException(
                   $"Failed to deserialize list response from Dolibarr for endpoint '{endpoint}'");
    }

    public async Task<string> PostAsync(string endpoint, object requestBody)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(requestBody)
        };
        AddDolibarrTokenHeader(request);
        var response = await httpClient.SendAsync(request);
        await EnsureSuccessOrThrowAsync(response, endpoint);

        // porque devuelve el id como response
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> PutAsync(string endpoint, object requestBody)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = JsonContent.Create(requestBody)
        };
        AddDolibarrTokenHeader(request);
        var response = await httpClient.SendAsync(request);
        await EnsureSuccessOrThrowAsync(response, endpoint);

        return await response.Content.ReadAsStringAsync();
    }

    private void AddDolibarrTokenHeader(HttpRequestMessage request)
    {
        var dolibarrToken = tokenCacheService.GetDolibarrToken();
        if (!string.IsNullOrEmpty(dolibarrToken))
        {
            request.Headers.Add("DOLAPIKEY", dolibarrToken);
        }
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