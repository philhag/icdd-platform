namespace IcddWebApp.Services.Models.Requests
{
    public class FolderDocumentRequest
    {
        public string FolderName { get; set; }
        public string? Schema { get; set; }
        public string? SchemaVersion { get; set; }
        public string? SchemaSubset { get; set; }

        public FolderDocumentRequest() { }

        public FolderDocumentRequest(string folderName, string? schema, string? schemaVersion, string? schemaSubset)
        {
            FolderName = folderName;

            if (schema != null)
                Schema = schema;

            if (schemaVersion != null)
                SchemaVersion = schemaVersion;

            if (schemaSubset != null)
                SchemaSubset = schemaSubset;
        }
    }
}
