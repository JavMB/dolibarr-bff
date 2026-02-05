using System.ComponentModel.DataAnnotations;
using DoliMiddlewareApi.Dtos;
using DoliMiddlewareApi.Dtos.command;
using DoliMiddlewareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoliMiddlewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController(InvoiceService invoiceService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<InvoiceDto>>> GetInvoices(
        [FromQuery] int limit = 50,
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery] string? status = null,
        [FromQuery][StringLength(100)] string? search = null)
    {
        var invoices = await invoiceService.GetInvoicesAsync(limit, page, status, search);
        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InvoiceDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDetailDto>> GetInvoice([Range(1, int.MaxValue)] int id)
    {
        var invoice = await invoiceService.GetInvoiceAsync(id);
        return Ok(invoice);
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)] // Devuelve el ID creado
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> CreateInvoice([FromBody] CreateInvoiceDto createInvoiceDto)
    {
        var invoiceId = await invoiceService.CreateInvoiceAsync(createInvoiceDto);
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
        await invoiceService.UpdateInvoiceAsync(id, updateInvoiceDto);
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
        var result = await invoiceService.AddInvoiceLineAsync(id, lineDto);
        return CreatedAtAction(nameof(GetInvoice), new { id }, result);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateInvoiceStatus(
        [Range(1, int.MaxValue)] int id,
        [FromBody] UpdateInvoiceStatusDto updateStatusDto)
    {
        await invoiceService.ChangeInvoiceStatusAsync(id, updateStatusDto.Status);
        return NoContent();
    }

    [HttpPost("{id:int}/validate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateInvoice([Range(1, int.MaxValue)] int id)
    {
        await invoiceService.ValidateInvoiceAsync(id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteInvoice([Range(1, int.MaxValue)] int id)
    {
        await invoiceService.DeleteInvoiceAsync(id);
        return NoContent();
    }

    [HttpDelete("{invoiceId:int}/lines/{lineId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteInvoiceLine(
        [Range(1, int.MaxValue)] int invoiceId,
        [Range(1, int.MaxValue)] int lineId)
    {
        await invoiceService.DeleteInvoiceLineAsync(invoiceId, lineId);
        return NoContent();
    }
}
