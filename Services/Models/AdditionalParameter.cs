using System.ComponentModel.DataAnnotations;

namespace IcddWebApp.Services.Models
{
    public class AdditionalParameter
    {
        [Required]
        public string Key { get; set; }
        [Required]
        public string Value { get; set; }
        [Required]
        public bool Mandatory { get; set; }

        public AdditionalParameter(string key, string value, bool mandatory)
        {
            Key = key;
            Value = value;
            Mandatory = mandatory;
        }
    }
}