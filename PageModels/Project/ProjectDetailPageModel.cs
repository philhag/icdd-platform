using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.PageModels.Project
{
    public class ProjectDetailPageModel
    {
        public Services.Models.Project Project { get; set; }

        public ProjectDetailPageModel(Services.Models.Project project)
        {
            Project = project;
        }
    }
}
