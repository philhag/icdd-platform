namespace IcddWebApp.Services.Models.Requests
{
    public class DatabaseRequest
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseType { get; set; }
        public string QueryLanguage { get; set; }
        public string? InternalMappingDocument { get; set; }

        public string? Schema { get; set; }
        public string? SchemaVersion { get; set; }
        public string? SchemaSubset { get; set; }

        public DatabaseRequest() { }

        public DatabaseRequest(string connectionString, string dbName, string dbType, string dbQueryLanguage, string? schema, string? schemaVersion, string? schemaSubset, string? internalMappingDocument)
        {
            ConnectionString = connectionString;
            DatabaseName = dbName;
            DatabaseType = dbType;
            QueryLanguage = dbQueryLanguage;

            if (schema != null)
                Schema = schema;

            if (schemaVersion != null)
                SchemaVersion = schemaVersion;

            if (schemaSubset != null)
                SchemaSubset = schemaSubset;

            if (internalMappingDocument != null)
                InternalMappingDocument = internalMappingDocument;
        }
    }
}
