using Microsoft.AspNetCore.Http;

namespace IcddWebApp.Services.Models.Requests
{
    public class EncryptedDocumentRequest
    {
        public IFormFile File { get; set; }
        public string EncryptionAlgorithm { get; set; }
        public string? Schema { get; set; }
        public string? SchemaVersion { get; set; }
        public string? SchemaSubset { get; set; }

        public EncryptedDocumentRequest() { }

        public EncryptedDocumentRequest(IFormFile file, string encryptionAlgorithm, string? schema, string? schemaVersion, string? schemaSubset)
        {
            File = file;
            EncryptionAlgorithm = encryptionAlgorithm;

            if (schema != null)
                Schema = schema;

            if (schemaVersion != null)
                SchemaVersion = schemaVersion;

            if (schemaSubset != null)
                SchemaSubset = schemaSubset;
        }
    }
}
