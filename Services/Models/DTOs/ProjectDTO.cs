using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.Services.Models.DTOs
{
    public class ProjectDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public ProjectDTO () { }
        public ProjectDTO(Project project)
        {
            Id = project.Id;
            Name = project.Name;
            Created = project.Created;
            Modified = project.Modified;
        }
    }
}
