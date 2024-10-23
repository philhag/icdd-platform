using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.Services.Models.Update
{
    public class LinksetMetadataUpdate
    {
        public string Name { get; set; }


        public LinksetMetadataUpdate() { }

        public LinksetMetadataUpdate(string name)
        {
            Name = name;
        }
    }
}
