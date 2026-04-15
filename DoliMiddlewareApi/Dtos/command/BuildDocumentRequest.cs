namespace DoliMiddlewareApi.Dtos.command
{
    public class BuildDocumentRequest
    {
        public string modulepart { get; set; } = default!;
        public string? original_file { get; set; }
        public string? doctemplate { get; set; }
        public string? langcode { get; set; }


    }
}
