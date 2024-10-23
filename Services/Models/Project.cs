using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using IcddWebApp.Services.Models.Authentication;
using IcddWebApp.WebApplication.Environment;

namespace IcddWebApp.Services.Models
{
    public class Project
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        [JsonIgnore]
        public List<User> Users { get; set; } = new List<User>();
        public List<ContainerMetadata> Containers { get; set; } = new List<ContainerMetadata>();

        public Project() { }

        public Project(string name, List<User>? users, List<ContainerMetadata>? containers)
        {
            Id = GuidEncoder.Encode(Guid.NewGuid());
            Name = name;
            Created = DateTime.Now;
            Modified = DateTime.Now;
            if (users != null)
                Users = users;
            if (containers != null)
                Containers = containers;
        }
    }
}
