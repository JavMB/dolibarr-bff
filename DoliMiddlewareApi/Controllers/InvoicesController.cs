using System.ComponentModel.DataAnnotations;
using DoliMiddlewareApi.Dtos;
using DoliMiddlewareApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoliMiddlewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceService _invoiceService;

    public InvoicesController(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<InvoiceDto>>> GetInvoices(
        [FromQuery] int limit = 50,
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] string? status = null)
    {
        var invoices = await _invoiceService.GetInvoicesAsync(limit, page, status);
        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InvoiceDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDetailDto>> GetInvoice([Range(1, int.MaxValue)] int id)
    {
        var invoice = await _invoiceService.GetInvoiceAsync(id);
        return Ok(invoice);
    }
}