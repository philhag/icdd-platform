using System.Collections.Generic;
using IIB.ICDD.Model;
using Newtonsoft.Json.Linq;

namespace IcddWebApp.Services.Models.DTOs
{
    public class InformationContainerDTO
    {
        public string ContainerId { get; set; }
        public JObject Description { get; set; }
        public List<JObject> Documents { get; set; } = new List<JObject>();
        public List<JObject> Linksets { get; set; } = new List<JObject>();

        public InformationContainerDTO() { }

        public InformationContainerDTO(InformationContainer container)
        {
            ContainerId = container.ContainerGuid;
            Description = container.ContainerDescription.ToJsonLD();
            foreach (var elem in container.Documents)
            {
                Documents.Add(elem.ToJsonLD());
            }

            foreach (var elem in container.Linksets)
            {
                Linksets.Add(elem.ToJsonLD());
            }
        }

        public InformationContainerDTO(InformationContainer container, string containerId)
        {
            ContainerId = containerId;
            Description = container.ContainerDescription.ToJsonLD();
            foreach (var elem in container.Documents)
            {
                Documents.Add(elem.ToJsonLD());
            }

            foreach (var elem in container.Linksets)
            {
                Linksets.Add(elem.ToJsonLD());
            }
        }
    }
}
