using System.Globalization;
using DoliMiddlewareApi.Dtos;
using DoliMiddlewareApi.Dtos.command;
using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Exceptions;
using DoliMiddlewareApi.Mappers;
using DoliMiddlewareApi.Services.Clients;

namespace DoliMiddlewareApi.Services;

public class InvoiceService(IDolibarrApiClient apiClient)
{
    public async Task<InvoiceDetailDto> GetInvoiceAsync(int id)
    {
        var data = await apiClient.GetResourceAsync<InvoiceDetailResponse>($"invoices/{id}");
        return InvoiceMapper.MapToInvoiceDetailDto(data);
    }

    public async Task<List<InvoiceDto>> GetInvoicesAsync(
        int limit = 50,
        int page = 1,
        string? status = null,
        string? search = null)
    {
        // empieza por 1 para el frontend
        var endpoint = $"invoices?limit={limit}&page={page - 1}";

        if (!string.IsNullOrEmpty(status))
        {
            endpoint += $"&status={status}";
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search=search.Trim().ToUpperInvariant();
            var filter = $"(t.ref:like:'{search}%')";
            endpoint += $"&sqlfilters={Uri.EscapeDataString(filter)}";
        }
        
        var dataList = await apiClient.GetCollectionAsync<InvoiceResponse>(endpoint);
        return dataList.Select(InvoiceMapper.MapToInvoiceDto).ToList();
    }

    public async Task<int> CreateInvoiceAsync(CreateInvoiceDto dto)
    {
        var requestBody = new
        {
            socid = dto.ClientId.ToString(),
            type = "0",
            statut = InvoiceMapper.ConvertStatusToDolibarr(dto.Status),
            date = ((DateTimeOffset)dto.Date).ToUnixTimeSeconds().ToString(),
            date_lim_reglement = dto.ExpireDate.HasValue
                ? ((DateTimeOffset)dto.ExpireDate.Value).ToUnixTimeSeconds().ToString()
                : null,
            @ref = dto.Reference,
            note_public = dto.NotePublic,
            note_private = dto.NotePrivate,
            lines = dto.Lines.Select(line => new
            {
                desc = line.Description,
                qty = line.Quantity.ToString(CultureInfo.InvariantCulture),
                subprice = line.UnitPrice.ToString(CultureInfo.InvariantCulture),
                tva_tx = line.TaxRate.ToString(CultureInfo.InvariantCulture)
            }).ToArray()
        };

        var responseBody = await apiClient.PostAsync("invoices", requestBody);

        return int.Parse(responseBody);
    }

    public async Task<string> AddInvoiceLineAsync(int invoiceId, CreateInvoiceLineDto lineDto)
    {
        var invoice = await apiClient.GetResourceAsync<InvoiceDetailResponse>($"invoices/{invoiceId}");
        if (invoice.statut != "0") throw new ForbiddenException("Solo se pueden añadir líneas a facturas en borrador");

        var requestBody = new
        {
            desc = lineDto.Description,
            qty = lineDto.Quantity.ToString(CultureInfo.InvariantCulture),
            subprice = lineDto.UnitPrice.ToString(CultureInfo.InvariantCulture),
            tva_tx = lineDto.TaxRate.ToString(CultureInfo.InvariantCulture)
        };

        return await apiClient.PostAsync($"invoices/{invoiceId}/lines", requestBody);
    }

    public async Task UpdateInvoiceAsync(int id, UpdateInvoiceDto dto)
    {
        // GET el JSON completo
        var current = await apiClient.GetResourceAsync<InvoiceDetailResponse>($"invoices/{id}");
        if (current.statut != "0") throw new ForbiddenException("Solo drafts");

        // Modifica solo campos que Dolibarr permite en PUT
        if (dto.Number != null) current.@ref = dto.Number;
        if (dto.NotePublic != null) current.note_public = dto.NotePublic;
        if (dto.NotePrivate != null) current.note_private = dto.NotePrivate;
        current.statut = InvoiceMapper.ConvertStatusToDolibarr(dto.Status);
        if (dto.ExpireDate.HasValue) current.date_lim_reglement = ((DateTimeOffset)dto.ExpireDate.Value).ToUnixTimeSeconds();

        // No tocar: date, socid, lines (Dolibarr no los cambia en PUT)

        await apiClient.PutAsync($"invoices/{id}", current);
    }
}
