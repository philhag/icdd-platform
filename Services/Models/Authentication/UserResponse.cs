using System.Collections.Generic;

namespace IcddWebApp.Services.Models.Authentication
{
    public class UserResponse
    {
        public string Username { get; set; }
        public string? Email { get; set; }
        public List<Project> Projects { get; set; }
        public IList<string>? Roles { get; set; }

        public UserResponse() { }

        public UserResponse(User user)
        {
            Username = user.UserName;
            Email = user.Email;
            if(user.Projects != null)
                Projects = user.Projects;
        }

        public UserResponse(User user, IList<string> roles)
        {
            Username = user.UserName;
            Email = user.Email;
            if (user.Projects != null)
                Projects = user.Projects;
            if (roles != null)
                Roles = roles;
        }
    }
}
