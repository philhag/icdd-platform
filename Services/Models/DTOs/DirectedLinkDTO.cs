using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.Services.Models.DTOs
{
    public class DirectedLinkDTO
    {
        public List<LeftLinkElementDTO> leftElements { get; set; }
        public List<RightLinkElementDTO> rightElements { get; set; }

        public DirectedLinkDTO () { }
        public DirectedLinkDTO(List<LeftLinkElementDTO> leftElements, List<RightLinkElementDTO> rightElements)
        {
            this.leftElements = leftElements;
            this.rightElements = rightElements;
        }
    }
}
