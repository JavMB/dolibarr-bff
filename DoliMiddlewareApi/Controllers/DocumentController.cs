using DoliMiddlewareApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DoliMiddlewareApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController(DocumentService documentService) : ControllerBase
    {

        [HttpGet("invoice/{invoiceRef}/pdf")]
        public async Task<IActionResult> GetInvoicePdf(string invoiceRef)
        {
            var (content, filename) = await documentService.BuildInvoicePdfAsync(invoiceRef);

            return File(content, "application/pdf", filename);
        }

    }
}
