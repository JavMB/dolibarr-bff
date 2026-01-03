using DoliMiddlewareApi.Dtos.command;
using DoliMiddlewareApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoliMiddlewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthApplicationService authAppService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] CreateTokenDto dto)
    {
        var result = await authAppService.LoginAsync(dto);
        return Ok(result);
    }
}