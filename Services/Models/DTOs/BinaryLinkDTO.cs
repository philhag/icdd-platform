using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.Services.Models.DTOs
{
    public class BinaryLinkDTO
    {
        public LeftLinkElementDTO leftElement { get; set; }
        public RightLinkElementDTO rightElement { get; set; }


        public BinaryLinkDTO() { }
        public BinaryLinkDTO(LeftLinkElementDTO leftElement, RightLinkElementDTO rightElement)
        {
            this.leftElement = leftElement;
            this.rightElement = rightElement;
        }
    }
}
