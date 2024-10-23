using System.Collections.Generic;
using IIB.ICDD.Model.Linkset.Link;
using Newtonsoft.Json.Linq;

namespace IcddWebApp.Services.Models.DTOs
{
    public class LinksetLinksDTO
    {
        public List<JObject> Links = new List<JObject>();

        public LinksetLinksDTO () { }

        public LinksetLinksDTO(IEnumerable<LsLink> links)
        {
            foreach(var link in links)
            {
                Links.Add(link.ToJsonLD());
            }
        }
    }
}
