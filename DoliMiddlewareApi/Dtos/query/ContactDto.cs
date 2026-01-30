namespace DoliMiddlewareApi.Dtos.query;

public class ContactDto
{
    public int Id { get; set; }

    public required string? Lastname { get; set; }

    public required string? Firstname { get; set; }

    public required string? Email { get; set; }

    public required string? PhonePro { get; set; }

    public required string? PhonePerso { get; set; }

    public required string? PhoneMobile { get; set; }

    public int ClientId { get; set; }
}
