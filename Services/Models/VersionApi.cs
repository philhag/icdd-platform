using System;
using System.ComponentModel.DataAnnotations;

namespace IcddWebApp.Services.Models
{
    public class VersionApi
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }


        public VersionApi() { }

        public VersionApi(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
        }
    }
}
