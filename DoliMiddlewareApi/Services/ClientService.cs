using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Dtos.query;
using DoliMiddlewareApi.Mappers;
using DoliMiddlewareApi.Services.Clients;

namespace DoliMiddlewareApi.Services;

public class ClientService(IDolibarrApiClient dolibarrApiClient)
{
    public async Task<List<ClientDto>> GetClientsAsync(
        int limit = 50,
        int page = 1)
    {
        var endpoint = $"thirdparties?limit={limit}&page={page - 1}";

        var clients = await dolibarrApiClient.GetCollectionAsync<ClientResponse>(endpoint);

        if (clients.Count == 0)
            return clients.Select(ClientMapper.MapToClientDtoWithoutContacts).ToList();

        var clientIds = string.Join(",", clients.Select(c => c.id).Where(id => !string.IsNullOrEmpty(id)));

        var contactsEndpoint = $"contacts?thirdparty_ids={clientIds}";
        var contacts = await dolibarrApiClient.GetCollectionAsync<ContactResponse>(contactsEndpoint);
        var contactDtos = contacts.Select(ContactMapper.MapToContactDto).ToList();

        return clients.Select(c => ClientMapper.MapToClientDto(c, contactDtos)).ToList();
    }
}