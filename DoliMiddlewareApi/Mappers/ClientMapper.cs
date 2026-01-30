using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Dtos.query;

namespace DoliMiddlewareApi.Mappers;

public class ClientMapper
{

    public static ClientDto MapToClientDto(ClientResponse clientResponse, List<ContactDto> contacts)
    {
        var clientId = int.TryParse(clientResponse.id, out int id) ? id : 0;

        return new ClientDto
        {
            Id = clientId,
            Name = clientResponse.name,
            CodeClient = clientResponse.code_client,
            Contacts = contacts.Where(c => c.ClientId == clientId).ToList()
        };
    }

    public static ClientDto MapToClientDtoWithoutContacts(ClientResponse clientResponse)
    {
        return new ClientDto
        {
            Id = int.TryParse(clientResponse.id, out int id) ? id : 0,
            Name = clientResponse.name,
            CodeClient = clientResponse.code_client
        };
    }

}