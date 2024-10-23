using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.Services.Models.DTOs
{
    public class DirectedBinaryLinkDTO
    {
        public LeftLinkElementDTO leftElement { get; set; }
        public RightLinkElementDTO rightElement { get; set; }
        public string specialization { get; set; }

        public DirectedBinaryLinkDTO() { }
        public DirectedBinaryLinkDTO(LeftLinkElementDTO leftElement, RightLinkElementDTO rightElement, string spec)
        {
            this.leftElement = leftElement;
            this.rightElement = rightElement;
            specialization = spec;
        }
    }
}
