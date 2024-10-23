using System.Collections.Generic;
using IcddWebApp.Services.Models;

namespace IcddWebApp.PageModels.Container
{
    public class ContainerDeletePageModel
    {
        public Services.Models.Project Project { get; set; }
        public ContainerMetadata ContainerMetadata { get; set; }

        public ContainerDeletePageModel(Services.Models.Project project, ContainerMetadata containerMetadata)
        {
            Project = project;
            ContainerMetadata = containerMetadata;

        }
    }
}
