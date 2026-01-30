namespace DoliMiddlewareApi.Dtos.Dolibarr;

public class ClientResponse
{
    public string? id { get; set; }

    // Nombre comercial / razón social
    public string? name { get; set; }

    // Código cliente (CUxxxx)
    public string? code_client { get; set; }

    // Contacto
    public string? email { get; set; }
    public string? phone { get; set; }
    
}