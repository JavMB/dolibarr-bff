using System.ComponentModel.DataAnnotations;
using DoliMiddlewareApi.Dtos;
using DoliMiddlewareApi.Dtos.command;
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

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)] // Devuelve el ID creado
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> CreateInvoice([FromBody] CreateInvoiceDto createInvoiceDto)
    {
        var invoiceId= await _invoiceService.CreateInvoiceAsync(createInvoiceDto);
        return CreatedAtAction(nameof(GetInvoice), new { id = invoiceId }, invoiceId);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateInvoice([Range(1, int.MaxValue)] int id, [FromBody] UpdateInvoiceDto updateInvoiceDto)
    {
        await _invoiceService.UpdateInvoiceAsync(id, updateInvoiceDto);
        return NoContent();
    }

    [HttpPost("{id:int}/lines")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> AddInvoiceLine([Range(1, int.MaxValue)] int id, [FromBody] CreateInvoiceLineDto lineDto)
    {
        var result = await _invoiceService.AddInvoiceLineAsync(id, lineDto);
        return CreatedAtAction(nameof(GetInvoice), new { id }, result);
    }
}