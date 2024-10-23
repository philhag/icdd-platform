using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Services.Models.Enums;

namespace IcddWebApp.Services.Models.DTOs
{
    public class ContainerMetadataDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ContainerType Type { get; set; }
        public string Revision { get; set; }
        public string ProjectId { get; set; }
        public DateTime? Created { get; set; }
        public string? Description { get; set; }
        public DateTime? Uploaded { get; set; }
        public string? Creator { get; set; }
        public string? Sender { get; set; }
        public List<UserDTO>? Recipients = new List<UserDTO>();
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

        public ContainerMetadataDTO() { }
        public ContainerMetadataDTO(ContainerMetadata containerMetadata)
        {
            Id = containerMetadata.Id;
            Name = containerMetadata.Name;
            Type = containerMetadata.Type;
            Revision = containerMetadata.Revision;
            ProjectId = containerMetadata.ProjectId;
            Created = containerMetadata.Created;
            Description = containerMetadata.Description;
            Uploaded = containerMetadata.Uploaded;
            Creator = containerMetadata.Creator;
            Sender = containerMetadata.Sender;
            foreach (var user in containerMetadata.Recipients)
            {
                Recipients.Add(new UserDTO(user));
            }
            Modifier = containerMetadata.Modifier;
            Publisher = containerMetadata.Publisher;
            Modified = containerMetadata.Modified;
            Suitability = containerMetadata.Suitability;
            Version = containerMetadata.Version;
            VersionDescription = containerMetadata.VersionDescription;
            Status = containerMetadata.Status;
            Content = containerMetadata.Content;
            Linkset = containerMetadata.Linkset;
            MetadataSchema = containerMetadata.MetadataSchema;
            AdditionalParameters = containerMetadata.AdditionalParameters;
            SparqlQueries = containerMetadata.SparqlQueries;
        }
    }
}
