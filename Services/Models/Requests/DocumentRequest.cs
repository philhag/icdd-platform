using Microsoft.AspNetCore.Http;

namespace IcddWebApp.Services.Models.Requests
{
    public class DocumentRequest
    {
        public string? Description { get; set; }
        public string? Filename { get; set; }
        public string? FileExtension { get; set; }
        public string? MimeType { get; set; }
        public string? Schema { get; set; }
        public string? SchemaVersion { get; set; }
        public string? SchemaSubset { get; set; }

        public DocumentRequest() { }

        public DocumentRequest(string? description, string filename, string fileExtension, string mimeType, string? schema, string? schemaVersion, string? schemaSubset)
        {
            Filename = filename;
            FileExtension = fileExtension;
            MimeType = mimeType;
            if (description != null)
                Description = description;
            if (schema != null)
                Schema = schema;
            if (schemaVersion != null)
                SchemaVersion = schemaVersion;
            if (schemaSubset != null)
                SchemaSubset = schemaSubset;
        }
    }
}
