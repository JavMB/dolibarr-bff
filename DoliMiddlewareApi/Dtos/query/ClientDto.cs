namespace DoliMiddlewareApi.Dtos.query;

public class ClientDto
{
    public int Id { get; set; }
    public required string? Name { get; set; }
    public required string? CodeClient { get; set; }

    public List<ContactDto> Contacts { get; set; } = new();
}