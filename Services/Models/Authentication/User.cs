using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace IcddWebApp.Services.Models.Authentication
{
    public class User : IdentityUser
    {
        [JsonIgnore]
        public string RefreshToken { get; set; }
        [JsonIgnore]
        [Required]
        public DateTime RefreshTokenExpiration { get; set; }
        public string Organisation { get; set; }
        public string Description { get; set; }
        public List<Project> Projects { get; set; } = new List<Project>();

        public User() { }

        public User(string username, List<Project>? projects)
        {
            Id = Guid.NewGuid().ToString();
            UserName = username;
            if (projects != null)
                Projects = projects;
        }

        public User(string username, string email)
        {
            Id = Guid.NewGuid().ToString();
            UserName = username;
            Email = email;
        }

        public User(UserRegister user)
        {
            Id = Guid.NewGuid().ToString();
            UserName = user.Username;
        }
    }
}
