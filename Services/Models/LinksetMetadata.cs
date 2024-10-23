using System;
using System.ComponentModel.DataAnnotations;
using IIB.ICDD.Model.Container;

namespace IcddWebApp.Services.Models
{
    public class LinksetMetadata
    {
        [Key]
        public string Id { get; set; }
        [Key]
        public string ContainerInternalId { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Creator { get; set; }
        public DateTime? Created { get; set; }
        public string? Modifier { get; set; }
        public DateTime? Modified { get; set; }
        public string? Version { get; set; }
        public string? VersionDescription { get; set; }


        public LinksetMetadata() { }

        public LinksetMetadata(CtLinkset linkset, string containerInternalId)
        {
            Id = linkset.Guid;
            ContainerInternalId = containerInternalId;
            Name = linkset.FileName;
            Creator = linkset.Creator?.Name;
            Created = linkset.Creation;
            Version = linkset.VersionId;
            VersionDescription = linkset.VersionDescription;
        }
    }
}
