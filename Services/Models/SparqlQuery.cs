using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.Services.Models
{
    public class SparqlQuery
    {
        [Key]
        [Required]
        public string Id { get; set; }
        [Required]
        public string Query { get; set; }
        [Required]
        public DateTime GeneratedAt { get; set; }
        public string Name { get; set; }

        public SparqlQuery() { }

        public SparqlQuery(string query, string name = "")
        {
            Id = Guid.NewGuid().ToString();
            GeneratedAt = DateTime.Now;
            Query = query;
            Name = name;
        }
    }
}