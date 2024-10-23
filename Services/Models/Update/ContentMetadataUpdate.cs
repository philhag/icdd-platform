using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.Services.Models.Update
{
    public class ContentMetadataUpdate
    {
        public string Name { get; set; }
        public string? Schema { get; set; }
        public string? SchemaVersion { get; set; }
        public string? SchemaSubset { get; set; }
        public string? Description { get; set; }

        // Database Connection

        public string? MappingFile { get; set; }

        public ContentMetadataUpdate() { }

        public ContentMetadataUpdate(string name, string? schema, string? schemaversion, string? schemasubset, string? description, string? mappingFile)
        {
            Name = name;
            Schema = schema;
            SchemaVersion = schemaversion;
            SchemaSubset = schemasubset;
            Description = description;
            MappingFile = mappingFile;
        }
    }
}
