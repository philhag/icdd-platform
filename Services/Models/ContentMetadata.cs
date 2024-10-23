using System;
using System.ComponentModel.DataAnnotations;
using IIB.ICDD.Model.Container.Document;
using Newtonsoft.Json;

namespace IcddWebApp.Services.Models
{
    public class ContentMetadata
    {
        [Key]
        public string Id { get; set; }
        [Key]
        public string ContainerInternalId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [JsonIgnore]
        public string Location { get; set; }
        public string? Type { get; set; }
        public string? Schema { get; set; }
        public string? SchemaVersion { get; set; }
        public string? SchemaSubset { get; set; }
        public string? Description { get; set; }
        public string? Creator { get; set; }
        public DateTime? Created { get; set; }
        public string? Modifier { get; set; }
        public DateTime? Modified { get; set; }
        public string? Version { get; set; }
        public string? VersionDescription { get; set; }


        public ContentMetadata() { }

        public ContentMetadata(CtDocument document, string containerInternalId,string location, string? schema, string? schemaVersion, string? schemaSubset)
        {
            Id = document.Guid;
            ContainerInternalId = containerInternalId;
            Name = document.Name;
            Location = location;
            Type = document.FileFormat;
            Description = document.Description;
            Schema = schema;
            SchemaVersion = schemaVersion;
            SchemaSubset = schemaSubset;
            Creator = document.Creator?.Name;
            Created = document.Creation;
            Version = document.VersionId;
            VersionDescription = document.VersionDescription;
        }
    }
}
