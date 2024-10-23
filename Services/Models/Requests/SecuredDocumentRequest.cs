using Microsoft.AspNetCore.Http;

namespace IcddWebApp.Services.Models.Requests
{
    public class SecuredDocumentRequest
    {
        public IFormFile File { get; set; }
        public string Checksum { get; set; }
        public string ChecksumAlgorithm { get; set; }
        public string? Schema { get; set; }
        public string? SchemaVersion { get; set; }
        public string? SchemaSubset { get; set; }

        public SecuredDocumentRequest() { }

        public SecuredDocumentRequest(IFormFile file, string checksum, string checksumAlgorithm, string? schema, string? schemaVersion, string? schemaSubset)
        {
            File = file;
            Checksum = checksum;
            ChecksumAlgorithm = checksumAlgorithm;

            if (schema != null)
                Schema = schema;

            if (schemaVersion != null)
                SchemaVersion = schemaVersion;

            if (schemaSubset != null)
                SchemaSubset = schemaSubset;
        }
    }
}
