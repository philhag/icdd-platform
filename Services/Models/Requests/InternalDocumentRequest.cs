using Microsoft.AspNetCore.Http;

namespace IcddWebApp.Services.Models.Requests
{
    public class InternalDocumentRequest
    {
        public IFormFile File { get; set; }
        public string? Description { get; set; }
        public string? Schema { get; set; }
        public string? SchemaVersion { get; set; }
        public string? SchemaSubset { get; set; }

        public InternalDocumentRequest() { }

        public InternalDocumentRequest(IFormFile file, string? description, string? schema, string? schemaVersion, string? schemaSubset)
        {
            File = file;
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
