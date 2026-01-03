using DoliMiddlewareApi.Dtos.command;
using DoliMiddlewareApi.Services.Auth;

namespace DoliMiddlewareApi.Services;

public class AuthApplicationService(DolibarrAuthService dolibarrAuth, JwtTokenProvider jwtProvider, DolibarrTokenCacheService tokenCacheService)
{
    public async Task<LoginResponse> LoginAsync(CreateTokenDto dto)
    {
        var doliToken = await dolibarrAuth.AuthenticateAsync(dto);
        var sessionId = Guid.NewGuid().ToString();

        tokenCacheService.SetDolibarrToken(sessionId, doliToken, TimeSpan.FromMinutes(30));

        var jwt = jwtProvider.GenerateJwt(sessionId, dto.Username);

        return new LoginResponse { Token = jwt };
    }
}

public class LoginResponse
{
    public required string Token { get; set; }
}