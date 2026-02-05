namespace DoliMiddlewareApi.Dtos.Dolibarr;

public class ClientResponse
{
    public string? id { get; set; }

    // Nombre comercial / razón social
    public string? name { get; set; }

    // Código cliente (CUxxxx)
    public string? code_client { get; set; }

    // Tipo de entidad
    public string? typent_code { get; set; }

    // Estado
    public string? status { get; set; }

    // Contacto
    public string? email { get; set; }
    public string? phone { get; set; }

}