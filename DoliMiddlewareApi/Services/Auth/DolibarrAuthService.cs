using DoliMiddlewareApi.Dtos.command;
using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Services.Clients;
using System.Text.Json;

namespace DoliMiddlewareApi.Services.Auth;

public sealed class DolibarrAuthService(IDolibarrApiClient apiClient)
{
    public async Task<string> AuthenticateAsync(CreateTokenDto createTokenDto)
    {
        var responseString = await apiClient.PostAsync("login", new
        {
            login = createTokenDto.Username,
            password = createTokenDto.Password
        });

        if (string.IsNullOrEmpty(responseString))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        var response = JsonSerializer.Deserialize<TokenResponse>(responseString);
        if (response == null || string.IsNullOrEmpty(response.AccessToken))
        {
            throw new UnauthorizedAccessException("Respuesta inválida de Dolibarr");
        }

        return response.AccessToken;
    }
}