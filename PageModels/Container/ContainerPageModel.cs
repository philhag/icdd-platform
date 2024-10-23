using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using IcddWebApp.Services.Models;
using IcddWebApp.WebApplication.Environment;
using IIB.ICDD.Model;
using IIB.ICDD.Model.Container;
using IIB.ICDD.Model.Container.Document;
using IIB.ICDD.Model.Linkset.Link;
using IIB.ICDD.Model.PayloadTriples;
using Microsoft.Extensions.Configuration;

namespace IcddWebApp.PageModels.Container
{
    public class ContainerPageModel
    {
        public Services.Models.Project Project { get; set; }
        public ContainerMetadata ContainerMetadata { get; set; }
        public InformationContainer Container { get; set; }
        public IfcContext ContainerModelContext { get; set; }

        public ContainerPageModel(Services.Models.Project project, ContainerMetadata containerMetadata, InformationContainer container)
        {
            Project = project;
            ContainerMetadata = containerMetadata;
            Container = container;
            ContainerModelContext = new IfcContext(Container);
        }
        protected ContainerPageModel(ContainerPageModel subModel)
        {
            Project = subModel.Project;
            ContainerMetadata = subModel.ContainerMetadata;
            Container = subModel.Container;
            ContainerModelContext = subModel.ContainerModelContext;
        }
    }

    public class LinksetPageModel : ContainerPageModel
    {
        public CtLinkset Linkset { get; set; }
        public LinksetMetadata Metadata { get; set; }

        //public string LinksetRawData { get; set; }

        public LinksetPageModel(ContainerPageModel model, CtLinkset linkset, string workfolderPath) : base(model)
        {
            Linkset = linkset;
            Metadata = ContainerMetadata.Linkset.Find(ls => ls.Id == linkset.Guid);
            //var xml = System.IO.File.ReadAllText(Path.Combine(workfolderPath, Project.Id, ContainerMetadata.InternalId, "Payload Triples", Metadata.Name));
            //XmlDocument document = new XmlDocument();
            //document.Load(new StringReader(xml));

            //StringBuilder builder = new StringBuilder();
            //using (XmlTextWriter writer = new XmlTextWriter(new StringWriter(builder)))
            //{
            //    writer.Formatting = Formatting.Indented;
            //    document.Save(writer);
            //}
            //LinksetRawData = builder.ToString();
        }

        public LinksetPageModel(Services.Models.Project project, ContainerMetadata containerMetadata, InformationContainer container, CtLinkset linkset, string workfolderPath) : base(project, containerMetadata, container)
        {
            Linkset = linkset;
            Metadata = containerMetadata.Linkset.Find(ls => ls.Id == linkset.Guid);
            //var xml = System.IO.File.ReadAllText(Path.Combine(workfolderPath, Project.Id, ContainerMetadata.InternalId, "Payload Triples", Metadata.Name));
            //XmlDocument document = new XmlDocument();
            //document.Load(new StringReader(xml));

            //StringBuilder builder = new StringBuilder();
            //using (XmlTextWriter writer = new XmlTextWriter(new StringWriter(builder)))
            //{
            //    writer.Formatting = Formatting.Indented;
            //    document.Save(writer);
            //}
            //LinksetRawData = builder.ToString();
        }
    }

    public class LinkPageModel : LinksetPageModel
    {
        public LsLink Link { get; set; }

        public LinkPageModel(Services.Models.Project project, ContainerMetadata containerMetadata, InformationContainer container, CtLinkset linkset, string linkId, string workfolderPath) : base(project, containerMetadata, container, linkset, workfolderPath)
        {
            Link = Linkset.GetLink(linkId);
        }
    }

    public class DocumentPageModel : ContainerPageModel
    {
        public CtDocument Document { get; set; }
        public ContentMetadata Metadata { get; set; }

        public DocumentPageModel(ContainerPageModel model, CtDocument document) : base(model)
        {
            Document = document;
            Metadata = ContainerMetadata.Content.Find(ls => ls.Id == document.Guid);
        }


        public DocumentPageModel(Services.Models.Project project, ContainerMetadata containerMetadata, InformationContainer container, CtDocument document) : base(project, containerMetadata, container)
        {
            Document = document;
            Metadata = containerMetadata.Content.Find(ls => ls.Id == document.Guid);
        }
    }

    public class OntologyPageModel : ContainerPageModel
    {
        public IcddOntology Ontology { get; set; }

        public OntologyPageModel(Services.Models.Project project, ContainerMetadata containerMetadata, InformationContainer container, string filename) : base(project, containerMetadata, container)
        {
            Ontology = container.UserDefinedOntologies.Find(ont => ont.GetFileName() == filename) ?? new IcddOntology(Path.Combine(container.GetOntologyFolder(), filename));
        }
    }

    public class PayloadTriplesPageModel : ContainerPageModel
    {
        public IcddPayloadTriples PayloadTriples { get; set; }

        public PayloadTriplesPageModel(Services.Models.Project project, ContainerMetadata containerMetadata, InformationContainer container, string filename) : base(project, containerMetadata, container)
        {
            PayloadTriples = container.PayloadTriples.Find(ont => ont.GetFileName() == filename);
        }
    }
}
