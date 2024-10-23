using System.Collections.Generic;
using IcddWebApp.Services.Models.Authentication;

namespace IcddWebApp.PageModels.Project
{
    public class ProjectPageModel
    {
        public User User { get; set; }

        public ProjectPageModel(User activeUser)
        {
            User = activeUser;
        }
    }
}
