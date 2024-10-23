using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.Services.Models.DTOs
{
    public class RightLinkElementDTO
    {
        public string hasDocument { get; set; }
        public IdentifierDTO hasIdentifier { get; set; }

        public RightLinkElementDTO () { }
        public RightLinkElementDTO(string document, IdentifierDTO identifier)
        {
            hasDocument = document;
            hasIdentifier = identifier;
        }
    }
}