namespace IcddWebApp.Services.Models.Requests
{
    public class ExternalDocumentRequest
    {
        public string DocumentUri { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string MimeType { get; set; }

        public string? Description { get; set; }
        public string? Schema { get; set; }
        public string? SchemaVersion { get; set; }
        public string? SchemaSubset { get; set; }

        public ExternalDocumentRequest() { }

        public ExternalDocumentRequest(string documentUri, string? description, string fileName, string fileExtension, string mimeType, string? schema, string? schemaVersion, string? schemaSubset)
        {
            DocumentUri = documentUri;
            if (description != null)
                Description = description;
            FileName = fileName;
            FileExtension = fileExtension;
            MimeType = mimeType;

            if (schema != null)
                Schema = schema;

            if (schemaVersion != null)
                SchemaVersion = schemaVersion;

            if (schemaSubset != null)
                SchemaSubset = schemaSubset;
        }
    }
}
