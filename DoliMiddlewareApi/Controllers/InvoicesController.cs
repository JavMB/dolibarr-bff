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
    public async Task<ActionResult<InvoiceDetailDto>> GetInvoice([Range(1, int.MaxValue)] int id)
    {
        var invoice = await invoiceService.GetInvoiceAsync(id);
        return Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateInvoice([FromBody] CreateInvoiceDto createInvoiceDto)
    {
        var invoiceId = await invoiceService.CreateInvoiceAsync(createInvoiceDto);
        return CreatedAtAction(nameof(GetInvoice), new { id = invoiceId }, invoiceId);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateInvoice([Range(1, int.MaxValue)] int id, [FromBody] UpdateInvoiceDto updateInvoiceDto)
    {
        await invoiceService.UpdateInvoiceAsync(id, updateInvoiceDto);
        return NoContent();
    }

    [HttpPost("{id:int}/lines")]
    public async Task<ActionResult<string>> AddInvoiceLine([Range(1, int.MaxValue)] int id, [FromBody] CreateInvoiceLineDto lineDto)
    {
        var result = await invoiceService.AddInvoiceLineAsync(id, lineDto);
        return CreatedAtAction(nameof(GetInvoice), new { id }, result);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateInvoiceStatus(
        [Range(1, int.MaxValue)] int id,
        [FromBody] UpdateInvoiceStatusDto updateStatusDto)
    {
        await invoiceService.ChangeInvoiceStatusAsync(id, updateStatusDto.Status);
        return NoContent();
    }

    [HttpPost("{id:int}/validate")]
    public async Task<IActionResult> ValidateInvoice([Range(1, int.MaxValue)] int id)
    {
        await invoiceService.ValidateInvoiceAsync(id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteInvoice([Range(1, int.MaxValue)] int id)
    {
        await invoiceService.DeleteInvoiceAsync(id);
        return NoContent();
    }

    [HttpDelete("{invoiceId:int}/lines/{lineId:int}")]
    public async Task<IActionResult> DeleteInvoiceLine(
        [Range(1, int.MaxValue)] int invoiceId,
        [Range(1, int.MaxValue)] int lineId)
    {
        await invoiceService.DeleteInvoiceLineAsync(invoiceId, lineId);
        return NoContent();
    }
}
