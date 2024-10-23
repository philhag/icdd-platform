using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Services.Models.Authentication;

namespace IcddWebApp.PageModels.Admin
{
    public class ManageRolesPageModel
    {
        public List<IdentityRole> AllRoles { get; set; }
        public List<User> AllUsers { get; set; }
        public Dictionary<string, List<User>> RoleUsers { get; set; }

        public ManageRolesPageModel(List<IdentityRole> allRoles, List<User> allUsers, Dictionary<string, List<User>> roleUsers)
        {
            AllRoles = allRoles;
            AllUsers = allUsers;
            RoleUsers = roleUsers;
        }
    }
}
