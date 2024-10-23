using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using IcddWebApp.Services.Models.Authentication;
using IcddWebApp.Services.Models.Enums;
using IcddWebApp.Services.Models.Requests;
using IIB.ICDD.Model;
using Newtonsoft.Json;

namespace IcddWebApp.Services.Models
{
    public class ContainerMetadata
    {
        // mandatory
        [Required]
        public string Id { get; set; }
        [Key]
        [JsonIgnore]
        public string InternalId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public ContainerType Type { get; set; }
        [Required]
        public string Revision { get; set; }
        [Required]
        public string ProjectId { get; set; }

        // optional
        public DateTime? Created { get; set; }
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }
        public DateTime? Uploaded { get; set; }
        public string? Creator { get; set; }
        public string? Sender { get; set; }
        public List<User>? Recipients = new List<User>();
        public string? Modifier { get; set; }
        public string? Publisher { get; set; }
        public DateTime? Modified { get; set; }
        public ContainerSuitability? Suitability { get; set; }
        public string? Version { get; set; }
        public string? VersionDescription { get; set; }
        public ContainerStatus? Status { get; set; }
        public List<ContentMetadata> Content { get; set; } = new List<ContentMetadata>();
        public List<LinksetMetadata> Linkset { get; set; } = new List<LinksetMetadata>();
        public string? MetadataSchema { get; set; }
        public List<AdditionalParameter>? AdditionalParameters { get; set; }
        public List<SparqlQuery> SparqlQueries { get; set; } = new List<SparqlQuery>();


        public ContainerMetadata() { }

        public ContainerMetadata(InformationContainer container, string projectId, ContainerType type, List<User> recipients, ContainerMetadataRequest? metadata, string projectPath)
        {
            Id = container.ContainerGuid;
            InternalId = Guid.NewGuid().ToString();
            Name = container.ContainerName;
            Type = type;
            if (metadata != null)
            {
                Revision = metadata.Revision ?? "0";
                Suitability = metadata.Suitability ?? ContainerSuitability.DEFAULT;
                Status = metadata.Status ?? ContainerStatus.WORK_IN_PROGRESS;
                MetadataSchema = metadata.MetadataSchema;
                AdditionalParameters = metadata.AdditionalParameters;
                if (metadata.Status == ContainerStatus.PUBLISHED)
                    Publisher = container.ContainerDescription.Publisher?.Name;
            }
            else
            {
                Revision = "0";
                Status = ContainerStatus.WORK_IN_PROGRESS;
                Suitability = ContainerSuitability.DEFAULT;
            }
            Created = container.ContainerDescription.Creation;
            Description = container.ContainerDescription.Description;
            Uploaded = DateTime.Now;
            if (container.ContainerDescription.Creator != null)
            {
                Creator = container.ContainerDescription.Creator.Name;
                Sender = container.ContainerDescription.Creator.Name;
            }
            
            Recipients = recipients;
            //Modifier = container.ContainerDescription.Modifier.Name;
            //ModificationDate = container.ContainerDescription.Modification;
            //Publisher = container.ContainerDescription.Publisher.Name;
            Version = container.ContainerDescription.VersionId;
            VersionDescription = container.ContainerDescription.VersionDescription;
            ProjectId = projectId;
            foreach (var elem in container.Documents)
            {
                Content.Add(new ContentMetadata(elem, InternalId, Path.Combine(projectPath, InternalId, "payload documents", $"{elem.Name}"), null, null, null));
            }
            foreach (var elem in container.Linksets)
            {
                Linkset.Add(new LinksetMetadata(elem, InternalId));
            }
        }

        public ContainerMetadata Update(ContainerMetadata update)
        {
            Name = update.Name;
            Revision = update.Revision ?? "0";
            Description = update.Description;
            Sender = update.Sender;
            Modifier = update.Modifier;
            Modified = DateTime.Now;
            Suitability = update.Suitability;
            Status = update.Status;
            if (update.Status == ContainerStatus.PUBLISHED)
                Publisher = update.Publisher;
            MetadataSchema = update.MetadataSchema;
            AdditionalParameters = update.AdditionalParameters;
            return this;
        }
    }
}