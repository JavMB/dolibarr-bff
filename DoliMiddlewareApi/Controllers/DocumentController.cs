using DoliMiddlewareApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DoliMiddlewareApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController(DocumentService documentService) : ControllerBase
    {

        [HttpPut("invoice/{invoiceRef}/build")]
        public async Task<IActionResult> BuildInvoicePdf(string invoiceRef)
        {
            await documentService.BuildInvoicePdfAsync(invoiceRef);

            var exists = await documentService.ExistsAsync(invoiceRef);

            return Ok(new
            {
                generated = exists,
                invoiceRef
            });
        }

    }
}
