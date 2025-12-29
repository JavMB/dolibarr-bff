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
    public async Task<IActionResult> GetInvoices()
    {
        var invoices = await _dolibarrClient.GetInvoicesAsync();
        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var invoice = await _dolibarrClient.GetInvoiceAsync(id);
        return Ok(invoice);
    }
}
