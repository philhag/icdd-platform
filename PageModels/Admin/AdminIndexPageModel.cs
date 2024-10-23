using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Services.Models.Authentication;

namespace IcddWebApp.PageModels.Admin
{
    public class AdminIndexPageModel
    {
        public List<User> AllUsers { get; set; }

        public AdminIndexPageModel(List<User> allusers)
        {
            AllUsers = allusers;
        }
    }
}
