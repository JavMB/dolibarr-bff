using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Dtos.query;

namespace DoliMiddlewareApi.Mappers;

public class ContactMapper
{
    public static ContactDto MapToContactDto(ContactResponse contactResponse)
    {
        return new ContactDto
        {
            Id = int.TryParse(contactResponse.id, out int id) ? id : 0,
            Lastname = contactResponse.lastname,
            Firstname = contactResponse.firstname,
            Email = contactResponse.email,
            PhonePro = contactResponse.phone_pro,
            PhonePerso = contactResponse.phone_perso,
            PhoneMobile = contactResponse.phone_mobile,
            ClientId = int.TryParse(contactResponse.fk_soc, out int clientId) ? clientId : 0
        };
    }
}
