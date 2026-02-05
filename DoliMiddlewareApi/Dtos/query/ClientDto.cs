namespace DoliMiddlewareApi.Dtos.query;

public class ClientDto
{
    public int Id { get; set; }
    public required string? Name { get; set; }
    public required string? CodeClient { get; set; }
    public string? TypentCode { get; set; }
    public string? Status { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public List<ContactDto> Contacts { get; set; } = new();
}