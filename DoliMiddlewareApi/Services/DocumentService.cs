using DoliMiddlewareApi.Dtos.command;
using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Services.Clients;
using System.Text.Json;

public class DocumentService(IDolibarrApiClient apiClient)
{
    public async Task<(byte[] content, string filename)> BuildInvoicePdfAsync(string invoiceRef)
    {
        var request = new BuildDocumentRequest
        {
            modulepart = "invoice",
            original_file = $"{invoiceRef}/{invoiceRef}.pdf",
            doctemplate = "crabe",
            langcode = "es_ES"
        };

        var response = await apiClient.PutAsync("documents/builddoc", request);

        var result = JsonSerializer.Deserialize<DolibarrPdfResponse>(response);

        if (result == null || string.IsNullOrEmpty(result.content))
            throw new Exception("Dolibarr no devolvió PDF");

        var bytes = Convert.FromBase64String(result.content);

        return (bytes, result.filename);
    }

}