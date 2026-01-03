using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace DoliMiddlewareApi.Services.Auth;

public class DolibarrTokenCacheService(IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
{
   
    public string? GetDolibarrToken()
    {
        var context = httpContextAccessor.HttpContext;
        {
            var sessionIdClaim = context?.User.Claims.FirstOrDefault(c => c.Type == "sessionId");
            if (sessionIdClaim != null && cache.TryGetValue(sessionIdClaim.Value, out string? dolibarrToken))
            {
                return dolibarrToken;
            }
        }
        return null;
    }
    
    public void SetDolibarrToken(string sessionId, string dolibarrToken, TimeSpan expiration)
    {
        cache.Set(sessionId, dolibarrToken, expiration);
    }
}