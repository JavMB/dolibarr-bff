using DoliMiddlewareApi.Dtos.command;
using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Exceptions;
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
            throw new UnauthorizedException("Credenciales inválidas");
        }

        var response = JsonSerializer.Deserialize<TokenResponse>(responseString);
        if (response == null || string.IsNullOrEmpty(response.success.token))
        {
            throw new UnauthorizedException("Respuesta inválida de Dolibarr");
        }

        return response.success.token;
    }
}