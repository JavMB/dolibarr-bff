using DoliMiddlewareApi.Dtos.command;
using DoliMiddlewareApi.Services.Clients;

namespace DoliMiddlewareApi.Services
{
    public class DocumentService(IDolibarrApiClient apiClient)
    {

        public async Task BuildInvoicePdfAsync(string invoiceRef)
        {
            var request = new BuildDocumentRequest
            {
                modulepart = "invoice",
                original_file = $"{invoiceRef}/{invoiceRef}.pdf",
                doctemplate = "crabe",
                langcode = "es_ES"
            };

            await apiClient.PutAsync("documents/builddoc",request);
        }

        public async Task<bool> ExistsAsync(string invoiceRef)
        {
            var file = $"¨{invoiceRef}/{invoiceRef}.pdf";

            var endpoint = $"documents?modulepart=invoice&id={invoiceRef}";

            var docs = await apiClient.GetCollectionAsync<string>(endpoint);

            return docs.Contains(file);

        }
    }
}
