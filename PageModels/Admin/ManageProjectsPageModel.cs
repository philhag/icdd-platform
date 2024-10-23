using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Services.Models.Authentication;

namespace IcddWebApp.PageModels.Admin
{
    public class ManageProjectsPageModel
    {
        public List<Services.Models.Project> AllProjects { get; set; }
        public List<User> AllUsers { get; set; }

        public ManageProjectsPageModel(List<Services.Models.Project> allProjects, List<User> allUsers)
        {
            AllProjects = allProjects;
            AllUsers = allUsers;
        }
    }
}
