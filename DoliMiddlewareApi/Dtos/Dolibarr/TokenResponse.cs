namespace DoliMiddlewareApi.Dtos.Dolibarr;

public class TokenResponse
{
    public required SuccessData success { get; set; }

    public class SuccessData
    {
        public int code { get; set; }
        public required string token { get; set; }
        public string? entity { get; set; }
        public string? message { get; set; }
    }
}