namespace DoliMiddlewareApi.Dtos.Dolibarr;

public class InvoiceResponse
{
    public string? id { get; set; }
    public string? @ref { get; set; }
    public long? date { get; set; }

    public long? date_lim_reglement { get; set; }

    public string? socid { get; set; }
    public string? total_ttc { get; set; }
    public string? remaintopay { get; set; }
    public string? statut { get; set; }

    public string? note_public { get; set; }
    public string? note_private { get; set; }
}