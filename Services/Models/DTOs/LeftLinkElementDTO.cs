using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.Services.Models.DTOs
{
    public class LeftLinkElementDTO
    {
        public string hasDocument { get; set; }
        public IdentifierDTO hasIdentifier { get; set; }

        public LeftLinkElementDTO () { }
        public LeftLinkElementDTO(string document, IdentifierDTO identifier)
        {
            hasDocument = document;
            hasIdentifier = identifier;
        }
    }
}
