using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.Services.Models.DTOs
{
    public class Directed1ToNLinkDTO
    {
        public LeftLinkElementDTO leftElement { get; set; }
        public List<RightLinkElementDTO> rightElements { get; set; }
        public string specialization { get; set; }

        public Directed1ToNLinkDTO() { }
        public Directed1ToNLinkDTO(LeftLinkElementDTO leftElement, List<RightLinkElementDTO> rightElements, string spec)
        {
            this.leftElement = leftElement;
            this.rightElements = rightElements;
            specialization = spec;
        }
    }
}
