namespace DoliMiddlewareApi.Services.Clients;

public interface IDolibarrApiClient
{
    Task<T> GetResourceAsync<T>(string endpoint) where T : class;
    Task<List<T>> GetCollectionAsync<T>(string endpoint) where T : class;
    Task<string> PostAsync(string endpoint, object requestBody);
    Task<string> PutAsync(string endpoint, object requestBody);
    Task DeleteAsync(string endpoint);
}
