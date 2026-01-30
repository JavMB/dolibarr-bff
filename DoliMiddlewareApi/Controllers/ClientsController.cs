using System.ComponentModel.DataAnnotations;
using DoliMiddlewareApi.Dtos.query;
using DoliMiddlewareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoliMiddlewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class ClientsController(ClientService clientService) : ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(List<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ClientDto>>> GetClientes(
        [FromQuery] int limit = 50,
        [FromQuery] [Range(1, int.MaxValue)] int page = 1)
    {
        var clients = await clientService.GetClientsAsync(limit, page);
        return Ok(clients);
    }
    
    
    
}
