using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Data;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.DTOs;
using IcddWebApp.Services.Models.Enums;
using IIB.ICDD.Model.Container;
using IIB.ICDD.Model.Linkset;
using IIB.ICDD.Model.Linkset.Link;
using IIB.ICDD.Parsing;
using IcddWebApp.Services.Models.Update;
using IIB.ICDD.Model;
using Microsoft.VisualBasic;
using System.Net;

namespace IcddWebApp.Services
{
    public class LinksetService : ILinksetService
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _workfolderPath;
        private readonly IContainerService _containerService;

        public LinksetService(DatabaseContext context, IConfiguration configuration, IContainerService containerService)
        {
            _context = context;
            _configuration = configuration;
            _workfolderPath = _configuration["WorkfolderPath"];
            _containerService = containerService;
        }

        public async Task<IEnumerable<LinksetMetadata>> GetContainerLinksets(string projectId, ContainerType containerType, string containerId, string containerVersion)
        {
            ContainerMetadata containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            return containerMetadata?.Linkset.ToList();
        }

        public async Task<CtLinkset> GetContainerLinkset(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId)
        {
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);
            var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
            var container = reader.Read();
            var linkset = container.GetLinkset(linksetId);
            return linkset;
        }

        public async Task<LinksetMetadata> GetContainerLinksetMetadata(string linksetId, string containerInternalId)
        {
            return await _context.LinksetMetadata.Where(x => x.Id == linksetId && x.ContainerInternalId == containerInternalId).SingleOrDefaultAsync(); ;
        }

        public async Task<CtLinkset> PostContainerLinkset(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string fileName)
        {
            ContainerMetadata containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);

            var projectPath = Path.Combine(_workfolderPath, projectId);
            var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
            var container = reader.Read();
            var linkset = container.CreateLinkset(fileName);

            if (linkset != null)
            {
                container.SaveRdf();
                var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                writer.Write(container);

                var creator = container.ContainerDescription.ContainsParty.Where(x => x.Name == username && x.GetType() == typeof(CtPerson)).SingleOrDefault();
                if (creator != null)
                    linkset.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
                else
                    linkset.Creator = new CtPerson(container, username, $"description for {username}");
                containerMetadata.Linkset.Add(new LinksetMetadata(linkset, containerMetadata.InternalId));
                await _context.SaveChangesAsync();

                return linkset;
            }
            else
            {
                return null;
            }
        }

        public async Task<IEnumerable<LsLink>> GetContainerLinksetLinks(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId)
        {
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);
            var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
            var container = reader.Read();
            var linkset = container.GetLinkset(linksetId);
            return linkset?.HasLinks;
        }

        public async Task<LsBinaryLink> PostContainerLinksetBinaryLink(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, BinaryLinkDTO linkElementDTO)
        {
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);

            if (containerMetadata != null)
            {
                var projectPath = Path.Combine(_workfolderPath, projectId);
                var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
                var container = reader.Read();
                var linkset = container.GetLinkset(linksetId);

                var resultLink = CreateBinaryLinkFromDto(container, linkset, linkElementDTO);
                if (resultLink == null)
                {
                    return null;
                }

                var creator = container.ContainerDescription.ContainsParty.SingleOrDefault(x => x.Name == username && x.GetType() == typeof(CtPerson));
                if (creator != null)
                    resultLink.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
                else
                    resultLink.Creator = new CtPerson(container, username, $"description for {username}");

                if (linkset != null)
                {
                    container.SaveRdf();
                    var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                    writer.Write(container);
                    return resultLink;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<List<LsBinaryLink>> PostContainerLinksetBinaryLinkList(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, List<BinaryLinkDTO> linkElementDTO)
        {
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);

            if (containerMetadata != null)
            {
                var projectPath = Path.Combine(_workfolderPath, projectId);
                var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
                var container = reader.Read();
                var linkset = container.GetLinkset(linksetId);
                var createdLinks = new List<LsBinaryLink>();

                foreach (var dto in linkElementDTO)
                {

                    var resultLink = CreateBinaryLinkFromDto(container, linkset, dto);
                    if (resultLink == null)
                    {
                        continue;
                    }

                    var creator = container.ContainerDescription.ContainsParty.SingleOrDefault(x => x.Name == username && x.GetType() == typeof(CtPerson));
                    resultLink.Creator = creator != null ? container.ContainerDescription.GetPersonById(creator.Guid) : new CtPerson(container, username, $"description for {username}");
                    createdLinks.Add(resultLink);
                }



                if (linkset != null)
                {
                    container.SaveRdf();
                    var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                    writer.Write(container);
                    return createdLinks;
                }
                else
                {
                    return createdLinks;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<LsDirectedLink> PostContainerLinksetDirectedLink(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, DirectedLinkDTO linkElementDTO)
        {
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);

            if (containerMetadata != null)
            {
                var projectPath = Path.Combine(_workfolderPath, projectId);
                var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
                var container = reader.Read();

                var linkset = container.GetLinkset(linksetId);

                var leftElems = new List<LsLinkElement>();
                var rightElems = new List<LsLinkElement>();

                foreach (var elem in linkElementDTO.leftElements)
                {
                    if (elem.hasIdentifier.type == LinkIdentifierType.STRING_BASED)
                        leftElems.Add(linkset.CreateLinkElement(container.GetDocument(elem.hasDocument), linkset.CreateStringBasedIdentifier(elem.hasIdentifier.identifier, elem.hasIdentifier.identifierField)));
                    else if (elem.hasIdentifier.type == LinkIdentifierType.URI_BASED)
                        leftElems.Add(linkset.CreateLinkElement(container.GetDocument(elem.hasDocument), linkset.CreateUriBasedIdentifier(elem.hasIdentifier.uri)));
                    else if (elem.hasIdentifier.type == LinkIdentifierType.QUERY_BASED)
                        leftElems.Add(linkset.CreateLinkElement(container.GetDocument(elem.hasDocument), linkset.CreateQueryBasedIdentifier(elem.hasIdentifier.queryExpression, elem.hasIdentifier.queryLanguage)));
                    else
                        leftElems.Add(linkset.CreateLinkElement(container.GetDocument(elem.hasDocument)));

                }

                foreach (var elem in linkElementDTO.rightElements)
                {
                    if (elem.hasIdentifier.type == LinkIdentifierType.STRING_BASED)
                        rightElems.Add(linkset.CreateLinkElement(container.GetDocument(elem.hasDocument), linkset.CreateStringBasedIdentifier(elem.hasIdentifier.identifier, elem.hasIdentifier.identifierField)));
                    else if (elem.hasIdentifier.type == LinkIdentifierType.URI_BASED)
                        rightElems.Add(linkset.CreateLinkElement(container.GetDocument(elem.hasDocument), linkset.CreateUriBasedIdentifier(elem.hasIdentifier.uri)));
                    else if (elem.hasIdentifier.type == LinkIdentifierType.QUERY_BASED)
                        rightElems.Add(linkset.CreateLinkElement(container.GetDocument(elem.hasDocument), linkset.CreateQueryBasedIdentifier(elem.hasIdentifier.queryExpression, elem.hasIdentifier.queryLanguage)));
                    else
                        rightElems.Add(linkset.CreateLinkElement(container.GetDocument(elem.hasDocument)));
                }

                var resultLink = linkset.CreateDirectedLink(leftElems, rightElems);
                if (resultLink == null)
                {
                    return null;
                }
                var creator = container.ContainerDescription.ContainsParty.Where(x => x.Name == username && x.GetType() == typeof(CtPerson)).SingleOrDefault();
                if (creator != null)
                    resultLink.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
                else
                    resultLink.Creator = new CtPerson(container, username, $"description for {username}");

                if (linkset != null)
                {
                    container.SaveRdf();
                    var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                    writer.Write(container);
                    return resultLink;
                }
                var creator = container.ContainerDescription.ContainsParty.Where(x => x.Name == username && x.GetType() == typeof(CtPerson)).SingleOrDefault();
                if (creator != null)
                    resultLink.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
                else
                    resultLink.Creator = new CtPerson(container, username, $"description for {username}");

                if (linkset != null)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<LsDirectedBinaryLink> PostContainerLinksetDirectedBinaryLink(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, DirectedBinaryLinkDTO linkElementDTO)
        {
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);

            if (containerMetadata != null)
            {
                var projectPath = Path.Combine(_workfolderPath, projectId);
                var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
                var container = reader.Read();

                var linkset = container.GetLinkset(linksetId);

                var leftElemDTO = linkElementDTO.leftElement;
                var rightElemDTO = linkElementDTO.rightElement;
                LsLinkElement leftElem = null;
                LsLinkElement rightElem = null;

                if (leftElemDTO.hasIdentifier.type == LinkIdentifierType.STRING_BASED)
                    leftElem = linkset.CreateLinkElement(container.GetDocument(leftElemDTO.hasDocument), linkset.CreateStringBasedIdentifier(leftElemDTO.hasIdentifier.identifier, leftElemDTO.hasIdentifier.identifierField));
                else if (leftElemDTO.hasIdentifier.type == LinkIdentifierType.URI_BASED)
                    leftElem = linkset.CreateLinkElement(container.GetDocument(leftElemDTO.hasDocument), linkset.CreateUriBasedIdentifier(leftElemDTO.hasIdentifier.uri));
                else if (leftElemDTO.hasIdentifier.type == LinkIdentifierType.QUERY_BASED)
                    leftElem = linkset.CreateLinkElement(container.GetDocument(leftElemDTO.hasDocument), linkset.CreateQueryBasedIdentifier(leftElemDTO.hasIdentifier.queryExpression, leftElemDTO.hasIdentifier.queryLanguage));
                else
                    leftElem = linkset.CreateLinkElement(container.GetDocument(leftElemDTO.hasDocument));


                if (rightElemDTO.hasIdentifier.type == LinkIdentifierType.STRING_BASED)
                    rightElem = linkset.CreateLinkElement(container.GetDocument(rightElemDTO.hasDocument), linkset.CreateStringBasedIdentifier(rightElemDTO.hasIdentifier.identifier, rightElemDTO.hasIdentifier.identifierField));
                else if (rightElemDTO.hasIdentifier.type == LinkIdentifierType.URI_BASED)
                    rightElem = linkset.CreateLinkElement(container.GetDocument(rightElemDTO.hasDocument), linkset.CreateUriBasedIdentifier(rightElemDTO.hasIdentifier.uri));
                else if (rightElemDTO.hasIdentifier.type == LinkIdentifierType.QUERY_BASED)
                    rightElem = linkset.CreateLinkElement(container.GetDocument(rightElemDTO.hasDocument), linkset.CreateQueryBasedIdentifier(rightElemDTO.hasIdentifier.queryExpression, rightElemDTO.hasIdentifier.queryLanguage));
                else
                    rightElem = linkset.CreateLinkElement(container.GetDocument(rightElemDTO.hasDocument));

                LsDirectedBinaryLink resultLink;
                if (linkElementDTO.specialization == "isidenticalto")
                    resultLink = linkset.CreateIsIdenticalTo(leftElem, rightElem);
                else if (linkElementDTO.specialization == "conflictswith")
                    resultLink = linkset.CreateConflictsWith(leftElem, rightElem);
                else if (linkElementDTO.specialization == "isalternativeto")
                    resultLink = linkset.CreateIsAlternativeTo(leftElem, rightElem);
                else
                    resultLink = linkset.CreateDirectedBinaryLink(leftElem, rightElem);
                if (resultLink == null)
                {
                    return null;
                }
                var creator = container.ContainerDescription.ContainsParty.Where(x => x.Name == username && x.GetType() == typeof(CtPerson)).SingleOrDefault();
                if (creator != null)
                    resultLink.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
                else
                    resultLink.Creator = new CtPerson(container, username, $"description for {username}");

                if (linkset != null)
                {
                    container.SaveRdf();
                    var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                    writer.Write(container);
                    return resultLink;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<LsDirected1ToNLink> PostContainerLinksetDirected1ToNLink(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, Directed1ToNLinkDTO linkElementDTO)
        {
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);

            if (containerMetadata != null)
            {
                var projectPath = Path.Combine(_workfolderPath, projectId);
                var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
                var container = reader.Read();

                var linkset = container.GetLinkset(linksetId);

                var leftElemDTO = linkElementDTO.leftElement;
                LsLinkElement leftElem = null;
                var rightElems = new List<LsLinkElement>();

                if (leftElemDTO.hasIdentifier.type == LinkIdentifierType.STRING_BASED)
                    leftElem = linkset.CreateLinkElement(container.GetDocument(leftElemDTO.hasDocument), linkset.CreateStringBasedIdentifier(leftElemDTO.hasIdentifier.identifier, leftElemDTO.hasIdentifier.identifierField));
                else if (leftElemDTO.hasIdentifier.type == LinkIdentifierType.URI_BASED)
                    leftElem = linkset.CreateLinkElement(container.GetDocument(leftElemDTO.hasDocument), linkset.CreateUriBasedIdentifier(leftElemDTO.hasIdentifier.uri));
                else if (leftElemDTO.hasIdentifier.type == LinkIdentifierType.QUERY_BASED)
                    leftElem = linkset.CreateLinkElement(container.GetDocument(leftElemDTO.hasDocument), linkset.CreateQueryBasedIdentifier(leftElemDTO.hasIdentifier.queryExpression, leftElemDTO.hasIdentifier.queryLanguage));
                else
                    leftElem = linkset.CreateLinkElement(container.GetDocument(leftElemDTO.hasDocument));

                foreach (var elem in linkElementDTO.rightElements)
                {
                    if (elem.hasIdentifier.type == LinkIdentifierType.STRING_BASED)
                        rightElems.Add(linkset.CreateLinkElement(container.GetDocument(elem.hasDocument), linkset.CreateStringBasedIdentifier(elem.hasIdentifier.identifier, elem.hasIdentifier.identifierField)));
                    else if (elem.hasIdentifier.type == LinkIdentifierType.URI_BASED)
                        rightElems.Add(linkset.CreateLinkElement(container.GetDocument(elem.hasDocument), linkset.CreateUriBasedIdentifier(elem.hasIdentifier.uri)));
                    else if (elem.hasIdentifier.type == LinkIdentifierType.QUERY_BASED)
                        rightElems.Add(linkset.CreateLinkElement(container.GetDocument(elem.hasDocument), linkset.CreateQueryBasedIdentifier(elem.hasIdentifier.queryExpression, elem.hasIdentifier.queryLanguage)));
                    else
                        rightElems.Add(linkset.CreateLinkElement(container.GetDocument(elem.hasDocument)));
                }

                LsDirected1ToNLink resultLink;
                if (linkElementDTO.specialization == "isspecialisedas")
                    resultLink = linkset.CreateIsSpecialisedAs(leftElem, rightElems);
                else if (linkElementDTO.specialization == "haspart")
                    resultLink = linkset.CreateHasPart(leftElem, rightElems);
                else if (linkElementDTO.specialization == "hasmember")
                    resultLink = linkset.CreateHasMember(leftElem, rightElems);
                else if (linkElementDTO.specialization == "supersedes")
                    resultLink = linkset.CreateSupersedes(leftElem, rightElems);
                else if (linkElementDTO.specialization == "iselaboratedby")
                    resultLink = linkset.CreateIsElaboratedBy(leftElem, rightElems);
                else if (linkElementDTO.specialization == "specialises")
                    resultLink = linkset.CreateSpecialises(leftElem, rightElems);
                else if (linkElementDTO.specialization == "ispartof")
                    resultLink = linkset.CreateIsPartOf(leftElem, rightElems);
                else if (linkElementDTO.specialization == "ismemberof")
                    resultLink = linkset.CreateIsMemberOf(leftElem, rightElems);
                else if (linkElementDTO.specialization == "issupersededby")
                    resultLink = linkset.CreateIsSupersededBy(leftElem, rightElems);
                else if (linkElementDTO.specialization == "elaborates")
                    resultLink = linkset.CreateElaborates(leftElem, rightElems);
                else if (linkElementDTO.specialization == "iscontrolledby")
                    resultLink = linkset.CreateIsControlledBy(leftElem, rightElems);
                else
                    resultLink = linkset.CreateDirected1ToNLink(leftElem, rightElems);
                if (resultLink == null)
                {
                    return null;
                }
                var creator = container.ContainerDescription.ContainsParty.Where(x => x.Name == username && x.GetType() == typeof(CtPerson)).SingleOrDefault();
                if (creator != null)
                    resultLink.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
                else
                    resultLink.Creator = new CtPerson(container, username, $"description for {username}");

                if (linkset != null)
                {
                    container.SaveRdf();
                    var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                    writer.Write(container);
                    return resultLink;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<LinksetMetadata> UpdateContainerLinkset(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, string modifier, LinksetMetadataUpdate updateMetadata)
        {
            var container = await _containerService.GetContainer(projectId, containerType, containerId, containerVersion);
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var linksetMetadata = await GetContainerLinksetMetadata(linksetId, containerMetadata.InternalId);

            var linkset = container.GetLinkset(linksetId);

            if (container != null && linksetMetadata != null && linkset != null)
            {

                linksetMetadata.Name = updateMetadata.Name;
                linkset.FileName = updateMetadata.Name;


                var containerModifier = (CtPerson)container.ContainerDescription.ContainsParty.Where(c => c.Name == modifier && c.GetType() == typeof(CtPerson)).SingleOrDefault();
                if (containerModifier != null)
                {
                    linksetMetadata.Modifier = modifier;
                    linkset.Modifier = containerModifier;
                }
                else
                {
                    linksetMetadata.Modifier = modifier;
                    linkset.Modifier = new CtPerson(container, modifier, $"description for {modifier}");
                }

                linksetMetadata.Modified = DateTime.Now;
                linkset.Modification = DateTime.Now;

                _context.LinksetMetadata.Update(linksetMetadata);
                _context.Entry(linksetMetadata).State = EntityState.Modified;

                try
                {
                    var meta = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
                    meta.Modifier = modifier;
                    meta.Modified = DateTime.Now;

                    await _containerService.UpdateContainer(modifier, projectId, containerType, containerId, containerVersion, meta);
                    await _context.SaveChangesAsync();
                    //linkset.SaveRdf();
                    container.SaveRdf();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContainerMetadataVersionExists(containerId, containerVersion))
                    {
                        return null;
                    }
                    else
                    {
                        throw new Exception("Could not update content metadata");
                    }
                }
                return linksetMetadata;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteContainerLinkset(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId)
        {
            var container = await _containerService.GetContainer(projectId, containerType, containerId, containerVersion);
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var linksetMetadata = await GetContainerLinksetMetadata(linksetId, containerMetadata.InternalId);

            if (container != null && linksetMetadata != null && container.DeleteLinkset(linksetId))
            {
                var projectPath = Path.Combine(_workfolderPath, projectId);
                container.SaveRdf();
                var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                writer.Write(container);
                _context.Entry(linksetMetadata).State = EntityState.Deleted;
                _context.LinksetMetadata.Remove(linksetMetadata);
                await _context.SaveChangesAsync();

                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task<bool> DeleteContainerLinksetLink(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, string linkId)
        {

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);

            string containerInternalId = null;

            if (containerVersion != null)
            {
                containerInternalId = await _context.ContainerMetadata
                   .Where(x => x.Id == containerId && x.Type == containerType && x.ProjectId == projectId && x.Version == containerVersion)
                   .Select(y => y.InternalId)
                   .SingleOrDefaultAsync();
            }
            else
            {
                containerInternalId = await _context.ContainerMetadata
                    .Where(x => x.Id == containerId && x.Type == containerType && x.ProjectId == projectId)
                    .OrderByDescending(z => z.Version)
                    .Select(y => y.InternalId)
                    .FirstOrDefaultAsync();
            }


            if (containerMetadata != null)
            {
                var projectPath = Path.Combine(_workfolderPath, projectId);
                var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
                var container = reader.Read();
                var linkset = container.GetLinkset(linksetId);
                var link = linkset.HasLinks.Where(x => x.Guid == linkId).SingleOrDefault();

                if (link != null && linkset.DeleteLink(link))
                {
                    container.SaveRdf();
                    var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                    writer.Write(container);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //public async Task<ContentFile> GetContainerLinksetAsFile(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId)
        //{
        //    var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
        //    var linksetMetadata = containerMetadata.Linkset
        //        .Where(x => x.Id == linksetId)
        //        .SingleOrDefault();

        //    if (linksetMetadata != null)
        //    {
        //        var content = new MemoryStream(new WebClient().DownloadData(Path.Combine(_workfolderPath, projectId, containerMetadata.InternalId, "Payload Triples", linksetMetadata.Name)));
        //        var contentType = "application/rdf+xml";
        //        var fileName = linksetMetadata.Name;
        //        return new ContentFile(content, contentType, fileName);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        #region Helpers

        private bool ContainerMetadataExists(string id)
        {
            return _context.ContainerMetadata.Any(e => e.Id == id);
        }
        private bool ContainerMetadataVersionExists(string id, string version)
        {
            return _context.ContainerMetadata.Any(e => e.Id == id && e.Version == version);
        }
        private bool LinksetMetadataExists(string id)
        {
            return _context.LinksetMetadata.Any(e => e.Id == id);
        }
        private bool ProjectExists(string id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        private LsBinaryLink CreateBinaryLinkFromDto(InformationContainer container, CtLinkset linkset, BinaryLinkDTO dataTransferObject)
        {
            LeftLinkElementDTO leftElemDto = dataTransferObject.leftElement;
            RightLinkElementDTO rightElemDto = dataTransferObject.rightElement;
            LsLinkElement leftElem = null;
            LsLinkElement rightElem = null;

            if (leftElemDto.hasIdentifier.type == LinkIdentifierType.STRING_BASED)
                leftElem = linkset.CreateLinkElement(container.GetDocument(leftElemDto.hasDocument), linkset.CreateStringBasedIdentifier(leftElemDto.hasIdentifier.identifier, leftElemDto.hasIdentifier.identifierField));
            else if (leftElemDto.hasIdentifier.type == LinkIdentifierType.URI_BASED)
                leftElem = linkset.CreateLinkElement(container.GetDocument(leftElemDto.hasDocument), linkset.CreateUriBasedIdentifier(leftElemDto.hasIdentifier.uri));
            else if (leftElemDto.hasIdentifier.type == LinkIdentifierType.QUERY_BASED)
                leftElem = linkset.CreateLinkElement(container.GetDocument(leftElemDto.hasDocument), linkset.CreateQueryBasedIdentifier(leftElemDto.hasIdentifier.queryExpression, leftElemDto.hasIdentifier.queryLanguage));
            else
                leftElem = linkset.CreateLinkElement(container.GetDocument(leftElemDto.hasDocument));


            if (rightElemDto.hasIdentifier.type == LinkIdentifierType.STRING_BASED)
                rightElem = linkset.CreateLinkElement(container.GetDocument(rightElemDto.hasDocument), linkset.CreateStringBasedIdentifier(rightElemDto.hasIdentifier.identifier, rightElemDto.hasIdentifier.identifierField));
            else if (rightElemDto.hasIdentifier.type == LinkIdentifierType.URI_BASED)
                rightElem = linkset.CreateLinkElement(container.GetDocument(rightElemDto.hasDocument), linkset.CreateUriBasedIdentifier(rightElemDto.hasIdentifier.uri));
            else if (rightElemDto.hasIdentifier.type == LinkIdentifierType.QUERY_BASED)
                rightElem = linkset.CreateLinkElement(container.GetDocument(rightElemDto.hasDocument), linkset.CreateQueryBasedIdentifier(rightElemDto.hasIdentifier.queryExpression, rightElemDto.hasIdentifier.queryLanguage));
            else
                rightElem = linkset.CreateLinkElement(container.GetDocument(rightElemDto.hasDocument));

            return linkset.CreateBinaryLink(leftElem, rightElem);
        }

        #endregion
    }
}
