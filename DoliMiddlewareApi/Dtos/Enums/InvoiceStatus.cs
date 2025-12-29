namespace DoliMiddlewareApi.Dtos.Enums;

public enum InvoiceStatus
{
    Draft = 0,
    Validated = 1,
    Paid = 2,
    Unknown = 3
}

public static class InvoiceStatusMapper
{
    public static InvoiceStatus FromDolibarrStatus(string status)
    {
        return status switch
        {
            "0" => InvoiceStatus.Draft,
            "1" => InvoiceStatus.Validated,
            "2" => InvoiceStatus.Paid,
            _ => InvoiceStatus.Unknown
        };
    }
}