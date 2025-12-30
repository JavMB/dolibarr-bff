using System.ComponentModel.DataAnnotations;
using DoliMiddlewareApi.Dtos;
using DoliMiddlewareApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoliMiddlewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly DolibarrApiClient _dolibarrClient;

    public InvoicesController(DolibarrApiClient dolibarrClient)
    {
        _dolibarrClient = dolibarrClient;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<InvoiceDto>>> GetInvoices(
        [FromQuery] int limit = 50,
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] string? status = null)
    {
        var invoices = await _dolibarrClient.GetInvoicesAsync(limit, page, status);
        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDetailDto>> GetInvoice(int id)
    {
        var invoice = await _dolibarrClient.GetInvoiceAsync(id);
        return Ok(invoice);
    }
}