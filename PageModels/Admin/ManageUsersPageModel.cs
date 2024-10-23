using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Services.Models.Authentication;

namespace IcddWebApp.PageModels.Admin
{
    public class ManageUsersPageModel
    {
        public List<User> AllUsers { get; set; }
        public Dictionary<string, List<string>> UserRoles { get; set; }

        public ManageUsersPageModel(List<User> allusers, Dictionary<string, List<string>> userRoles)
        {
            AllUsers = allusers;
            UserRoles = userRoles;
        }
    }
}
