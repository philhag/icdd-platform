using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IcddWebApp.Services.Models;
using Microsoft.AspNetCore.Identity;
using IcddWebApp.Data;
using IcddWebApp.Services.Models.Authentication;
using IcddWebApp.Services.Models.Enums;
using IcddWebApp.Services.Models.Requests;
using IIB.ICDD.Model;
using IIB.ICDD.Model.Container;
using IIB.ICDD.Model.Container.Document;
using IIB.ICDD.Parsing;
using System.Text.RegularExpressions;
using IIB.ICDD.Handling;
using IIB.ICDD.Logging;

namespace IcddWebApp.Services
{
    public class ContainerService : IContainerService
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _workfolderPath;
        private UserManager<User> _userManager;

        public ContainerService(DatabaseContext context, IConfiguration configuration, UserManager<User> userManager)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _workfolderPath = _configuration["WorkfolderPath"];
        }


        public async Task<IEnumerable<ContainerMetadata>> GetContainers(string projectId, ContainerType containerType)
        {
            return await _context.ContainerMetadata
                .Where(x => x.ProjectId == projectId && x.Type == containerType)
                .Include("Content")
                .Include("Linkset")
                .Include("Recipients")
                .Include("AdditionalParameters")
                .ToListAsync();
        }

        public async Task<InformationContainer> GetContainer(string projectId, ContainerType containerType, string containerId, string containerVersion)
        {
            var containerMetadata = await GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);
            var container = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd").Read();
            return container;
        }

        public async Task<ContainerMetadata> GetContainerMetadata(string projectId, ContainerType containerType, string containerId, string containerVersion)
        {
            ContainerMetadata containerMetadata = null;

            if (containerVersion != null)
            {
                containerMetadata = await _context.ContainerMetadata
                    .Where(x => x.Id == containerId && x.Type == containerType && x.ProjectId == projectId && x.Version == containerVersion)
                    .Include("Content")
                    .Include("Linkset")
                    .Include("Recipients")
                    .Include("AdditionalParameters")
                    .Include("SparqlQueries")
                    .SingleOrDefaultAsync();
            }
            else
            {
                containerMetadata = await _context.ContainerMetadata
                    .Where(x => x.Id == containerId && x.Type == containerType && x.ProjectId == projectId)
                    .OrderByDescending(z => z.Version)
                    .Include("Content")
                    .Include("Linkset")
                    .Include("Recipients")
                    .Include("AdditionalParameters")
                    .Include("SparqlQueries").FirstOrDefaultAsync();
                //containerMetadata = data.Aggregate((max, x) => int.Parse(x.Version) > int.Parse(max.Version) ? x : max);
            }
            return containerMetadata;
        }

        public async Task<InformationContainer> PostContainer(string username, string projectId, ContainerType containerType, ContainerMetadataFileRequest request)
        {
            if (request.File != null && request.File.FileName.EndsWith(".icdd"))
            {
                var projectPath = Path.Combine(_workfolderPath, projectId);
                if (!Directory.Exists(projectPath))
                    Directory.CreateDirectory(projectPath);

                var filePath = Path.Combine(projectPath, request.File.FileName);
                var recipientList = await _context.ContextUsers.Include("Projects").ToListAsync();
                var recipients = recipientList.Where(x => x.Projects.Where(x => x.Id == projectId).Any()).ToList();

                using (var fs = System.IO.File.Create(filePath))
                {
                    await request.File.CopyToAsync(fs);
                    fs.Flush();
                }

                var container = new IcddContainerReader(filePath, new IcddContainerReaderOptions(projectPath)).Read();

                if (container != null)//&& !ContentsExist(container.ContainerDescription.ContainsDocument.ToList()))
                {
                    var user = await _userManager.FindByNameAsync(username);
                    var containerUser = container.ContainerDescription.ContainsParty.Where(x => x.Name == username && x.GetType() == typeof(CtPerson)).SingleOrDefault();
                    if (containerUser != null)
                    {
                        if (containerUser.Description != user.Description)
                            container.ContainerDescription.GetPersonById(containerUser.Guid).Description = user.Description;
                    }
                    else
                    {
                        container.ContainerDescription.AddPerson(username, user.Description);
                    }

                    foreach (var person in container.ContainerDescription.ContainsParty)
                    {
                        var recipient = recipients.Where(x => x.UserName == person.Name).SingleOrDefault();
                        if (recipient != null && !recipients.Contains(recipient))
                            recipients.Add(recipient);
                    }

                    if (container != null && username != null)
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch (Exception)
                        {
                            throw new Exception("Could not remove old container file");
                        }

                        if (request != null && request.ContainerFileName != null)
                            container.ContainerName = request.ContainerFileName;

                        container.ContainerDescription.Description = request.ContainerDescription;

                        if (user.Organisation != null && request.Status == ContainerStatus.PUBLISHED)
                        {
                            var containerPublisher = (CtOrganisation)container.ContainerDescription.ContainsParty.Where(c => c.Name == user.Organisation && c.GetType() == typeof(CtOrganisation)).SingleOrDefault();
                            if (containerPublisher != null)
                                container.ContainerDescription.Publisher = containerPublisher;
                            else
                                container.ContainerDescription.Publisher = new CtOrganisation(container, user.Organisation, $"Organisation of {user.UserName}");
                        }
                        container.SaveRdf();
                        container.Repository.Commit("Container added " + request.ContainerFileName, username);
                        var containerMetadata = new ContainerMetadata(container, projectId, containerType, recipients, new ContainerMetadataRequest(request), projectPath);
                        _context.ContainerMetadata.Add(containerMetadata);
                        var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                        writer.Write(container);

                        try
                        {
                            Directory.Move(Path.Combine(projectPath, container.ContainerGuid), Path.Combine(projectPath, containerMetadata.InternalId));
                        }
                        catch (Exception)
                        {
                            throw new Exception("Could not rename container file");
                        }

                        await _context.SaveChangesAsync();
                        return container;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    File.Delete(filePath);
                    if (Directory.Exists(Path.Combine(projectPath, container.ContainerGuid)))
                        Directory.Delete(Path.Combine(projectPath, container.ContainerGuid), true);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<InformationContainer> PostEmptyContainer(string username, string projectId, ContainerType containerType, ContainerMetadataRequest? request)
        {
            var recipientList = await _context.ContextUsers.Include("Projects").ToListAsync();
            var recipients = recipientList.Where(x => x.Projects.Where(x => x.Id == projectId).Any()).ToList();
            var newContainer = new IcddContainerBuilder(new IcddContainerBuilderOptions(Path.Combine(_workfolderPath, projectId))).GetAssembledContainer();
            if (newContainer != null && username != null)
            {
                var user = await _userManager.FindByNameAsync(username);
                var creator = new CtPerson(newContainer, username, user.Description);
                newContainer.ContainerDescription.Creator = creator;
                if (request != null && request.ContainerDescription != null)
                    newContainer.ContainerDescription.Description = request.ContainerDescription;
                if (request != null && request.ContainerFileName != null)
                    newContainer.ContainerName = request.ContainerFileName;

                foreach (var person in newContainer.ContainerDescription.ContainsParty)
                {
                    var recipient = recipients.Where(x => x.UserName == person.Name).SingleOrDefault();
                    if (recipient != null)
                        recipients.Add(recipient);
                }
                var projectPath = Path.Combine(_workfolderPath, projectId);

                if (user.Organisation != null && request.Status == ContainerStatus.PUBLISHED)
                {
                    var containerPublisher = (CtOrganisation)newContainer.ContainerDescription.ContainsParty.Where(c => c.Name == user.Organisation && c.GetType() == typeof(CtOrganisation)).SingleOrDefault();
                    if (containerPublisher != null)
                        newContainer.ContainerDescription.Publisher = containerPublisher;
                    else
                        newContainer.ContainerDescription.Publisher = new CtOrganisation(newContainer, user.Organisation, $"Organisation of {user.UserName}");
                }
                newContainer.SaveRdf();
                newContainer.Repository.Commit("Empty container added " + request.ContainerFileName, username);
                var containerMetadata = new ContainerMetadata(newContainer, projectId, containerType, recipients, request, projectPath);
                _context.ContainerMetadata.Add(containerMetadata);
                var internalPath = Path.Combine(projectPath, containerMetadata.InternalId);

                if (!Directory.Exists(projectPath))
                {
                    Directory.CreateDirectory(projectPath);
                }
                var writer = new IcddContainerWriter($"{internalPath}.icdd", true);
                writer.Write(newContainer);
                try
                {
                    Directory.Move(Path.Combine(projectPath, newContainer.ContainerGuid), internalPath);
                }
                catch (Exception)
                {
                    throw new Exception("Cannot move container from path " + Path.Combine(projectPath, newContainer.ContainerGuid) + " to path " + internalPath);
                }
                await _context.SaveChangesAsync();
                return newContainer;
            }
            else
            {
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetContainerVersions(string projectId, ContainerType containerType, string containerId)
        {
            if (ContainerMetadataExists(containerId))
            {
                return await _context.ContainerMetadata
                       .Where(x => x.Id == containerId && x.ProjectId == projectId && x.Type == containerType)
                       .Select(y => y.Version)
                       .ToListAsync();
            }
            else
            {
                return null;
            }
        }

        public async Task<InformationContainer> PostContainerVersion(string username, string projectId, ContainerType containerType, string containerId, ContainerMetadataRequest? request)
        {
            if (ContainerMetadataExists(containerId))
            {
                var containerMetadata = await GetContainerMetadata(projectId, containerType, containerId, null);

                var projectPath = Path.Combine(_workfolderPath, projectId);
                var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
                var container = reader.Read();

                if (int.TryParse(containerMetadata.Version, out var newVersion))
                    newVersion += 1;

                if (!ContainerMetadataVersionExists(containerId, newVersion.ToString()) && username != null)
                {
                    var newContainer = container.NextVersion(newVersion.ToString(), $"Version {newVersion} of {container.ContainerName}", $"{Path.GetFileNameWithoutExtension(containerMetadata.Name)}-v{newVersion}.icdd", true, containerId);
                    if (request != null && request.ContainerFileName != null)
                        newContainer.ContainerName = request.ContainerFileName;
                    if (request != null && request.ContainerDescription != null)
                        newContainer.ContainerDescription.Description = request.ContainerDescription;
                    else
                        newContainer.ContainerDescription.Description = $"Version {newVersion} of {container.ContainerName}";

                    var user = await _userManager.FindByNameAsync(username);
                    var desc = "";
                    if (user.Description != null)
                        desc = user.Description;
                    var creator = (CtPerson)container.ContainerDescription.ContainsParty.Where(x => x.Name == username && x.GetType() == typeof(CtPerson)).SingleOrDefault();
                    if (creator != null)
                        newContainer.ContainerDescription.Creator = creator;
                    else
                        newContainer.ContainerDescription.Creator = new CtPerson(newContainer, username, desc);

                    newContainer.SaveRdf();
                    newContainer.Repository.Commit("Container version added " + newContainer.ContainerName, username);
                    var recipientList = await _context.ContextUsers.Include("Projects").ToListAsync();
                    var recipients = recipientList.Where(x => x.Projects.Where(x => x.Id == projectId).Any()).ToList();
                    var newContainerMetadata = new ContainerMetadata(newContainer, projectId, containerType, recipients, request, projectPath);
                    newContainerMetadata.Suitability = containerMetadata.Suitability;
                    newContainerMetadata.SparqlQueries = containerMetadata.SparqlQueries;


                    _context.ContainerMetadata.Add(newContainerMetadata);

                    var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{newContainerMetadata.InternalId}.icdd"), true);
                    writer.Write(newContainer);

                    try
                    {
                        Directory.Move(Path.Combine(projectPath, newContainer.ContainerGuid), Path.Combine(projectPath, newContainerMetadata.InternalId));
                    }
                    catch (Exception)
                    {
                        throw new Exception("Cannot move container from path " + Path.Combine(projectPath, newContainer.ContainerGuid) + " to path " + Path.Combine(projectPath, newContainerMetadata.InternalId));
                    }
                    await _context.SaveChangesAsync();
                    return newContainer;
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

        public async Task<ContainerFile> GetContainerAsFile(string projectId, ContainerType containerType, string containerId, string containerVersion)
        {
            ContainerMetadata containerMetadata = await GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var container = await GetContainer(projectId, containerType, containerId, containerVersion);

            if (containerMetadata != null && container != null)
            {
                var path = Path.Combine(_workfolderPath, projectId, $"{containerMetadata.InternalId}.icdd");
                var writer = new IcddContainerWriter(path, true);
                writer.Write(container);
                var content = new MemoryStream(new WebClient().DownloadData(path));
                var contentType = "application/zip";
                var fileName = containerMetadata.Name;
                if (containerMetadata.Name == ".icdd")
                    fileName = $"{containerId}-v{containerMetadata.Version}";

                return new ContainerFile(content, contentType, fileName);
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteContainer(string username, string projectId, ContainerType containerType, string containerId, string containerVersion)
        {
            ContainerMetadata containerMetadata = await GetContainerMetadata(projectId, containerType, containerId, containerVersion);

            if (containerMetadata != null)
            {
                var workfolderPath = Path.Combine(_workfolderPath, containerMetadata.ProjectId, containerMetadata.InternalId);
                try
                {
                    if (Directory.Exists(workfolderPath))
                    {
                        InformationContainer container = await GetContainer(projectId, containerType, containerId, containerVersion);

                        var filePath = Path.Combine(_workfolderPath, containerMetadata.ProjectId, $"{containerMetadata.InternalId}.icdd");
                        
                        if (File.Exists(filePath))
                            File.Delete(filePath);

                        if (Directory.Exists(container.PathToContainer))
                            container.Delete();

                        if (Directory.Exists(workfolderPath))
                            Directory.Delete(workfolderPath, true);
                    }
                }
                catch (Exception ex)
                {
                    var exception = new IcddException("Could not delete container file or workfolder", ex);
                    return false;
                }

                _context.Entry(containerMetadata).State = EntityState.Deleted;
                _context.ContainerMetadata.Remove(containerMetadata);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<ContainerMetadata> UpdateContainerRecipients(string username, string projectId, string containerId, string containerVersion, List<User> newRecipients)
        {
            var containerMetadata = await GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            containerMetadata.Recipients = newRecipients;
            _context.ContainerMetadata.Update(containerMetadata);
            _context.Entry(containerMetadata).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContainerMetadataVersionExists(containerId, containerVersion))
                {
                    return null;
                }
                else
                {
                    throw new Exception("Could not update container recipients");
                }
            }
            return containerMetadata;
        }

        public async Task<InformationContainer> ReadWorkfolderContainer(string projectId, string containerId, ContainerType containerType, string containerVersion)
        {
            var containerMetadata = await GetContainerMetadata(projectId, containerType, containerId, containerVersion);

            if (containerMetadata.InternalId != null)
            {
                var projectPath = Path.Combine(_workfolderPath, projectId);
                return new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd").Read();
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> AddParticipantToContainer(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, int type, string partName, string partDesc)
        {
            var container = await GetContainer(projectId, containerType, containerId, containerVersion);
            if (container != null)
            {
                if (type == 1)
                    container.ContainerDescription.AddPerson(partName, partDesc);
                else
                    container.ContainerDescription.AddOrganisation(partName, partDesc);
                container.SaveRdf();
                container.Repository.Commit("Participant added " + partName, username);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> PostOntologyToContainer(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string? webUrl, IFormFile? uploadFile)
        {
            InformationContainer container = await GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);

            if (container != null)
            {
                if (uploadFile != null)
                {
                    var tempFilePath = Path.Combine(projectPath, uploadFile.FileName);
                    using (var fs = System.IO.File.Create(tempFilePath))
                    {
                        await uploadFile.CopyToAsync(fs);
                        fs.Flush();
                    }
                    var fileExtension = Path.GetExtension(tempFilePath).ToLower();
                    if (isOntologyOrTriple(tempFilePath))
                    {
                        container.CreateOntology(tempFilePath);
                        File.Delete(tempFilePath);
                        container.Repository.Commit("Ontology added " + uploadFile.FileName, username);
                        return true;
                    }
                    else
                        return false;
                }
                else if (!string.IsNullOrEmpty(webUrl) && uploadFile == null)
                {
                    Uri uri = new Uri(webUrl);

                    string filename = System.IO.Path.GetFileName(uri.LocalPath);
                    var tempFilePath = Path.Combine(projectPath, filename);
                    WebClient client = new WebClient();
                    client.DownloadFile(webUrl, tempFilePath);
                    if (isOntologyOrTriple(tempFilePath))
                    {
                        container.CreateOntology(tempFilePath);
                        File.Delete(tempFilePath);
                        container.Repository.Commit("Ontology added " + filename, username);
                        return true;
                    }
                    else
                        return false;

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

        public async Task<bool> PostPayloadTriplesToContainer(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string? webUrl, IFormFile? uploadFile)
        {
            InformationContainer container = await GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);

            if (container != null)
            {
                if (uploadFile != null)
                {
                    var tempFilePath = Path.Combine(projectPath, uploadFile.FileName);
                    using (var fs = System.IO.File.Create(tempFilePath))
                    {
                        await uploadFile.CopyToAsync(fs);
                        fs.Flush();
                    }
                    container.CreatePayloadTriple(tempFilePath);
                    File.Delete(tempFilePath);
                    container.Repository.Commit("Payload triples added " + uploadFile.FileName, username);
                    return true;
                }
                else if (!string.IsNullOrEmpty(webUrl) && uploadFile == null)
                {
                    Uri uri = new Uri(webUrl);

                    string filename = System.IO.Path.GetFileName(uri.LocalPath);
                    var tempFilePath = Path.Combine(projectPath, filename);
                    WebClient client = new WebClient();
                    client.DownloadFile(webUrl, tempFilePath);
                    container.CreatePayloadTriple(tempFilePath);
                    File.Delete(tempFilePath);
                    container.Repository.Commit("Payload triples added " + filename, username);
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

        public async Task<ContainerMetadata> UpdateContainer(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, ContainerMetadata updateMetadata)
        {
            var container = await GetContainer(projectId, containerType, containerId, containerVersion);
            var containerMetadata = await GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);

            if (container != null && containerMetadata != null)
            {
                containerMetadata.Modifier = updateMetadata.Modifier;
                containerMetadata = containerMetadata.Update(updateMetadata);
                container = await ContainerUpdate(container, updateMetadata);
                _context.ContainerMetadata.Update(containerMetadata);
                _context.Entry(containerMetadata).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    container.SaveRdf();
                    container.Repository.Commit("Container metadata updated " + containerMetadata.Name, username);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContainerMetadataVersionExists(containerId, containerVersion))
                    {
                        return null;
                    }
                    else
                    {
                        throw new Exception("Could not update container metadata");
                    }
                }
                return containerMetadata;
            }
            else
            {
                return null;
            }
        }

        public async Task<ContentFile> GetContainerPayloadTriplesAsFile(string projectId, ContainerType containerType, string containerId, string containerVersion, string payloadFileName)
        {
            var containerMetadata = await GetContainerMetadata(projectId, containerType, containerId, containerVersion);

            if (containerMetadata != null)
            {
                var content = new MemoryStream(new WebClient().DownloadData(Path.Combine(_workfolderPath, projectId, containerMetadata.InternalId, "Payload Triples", payloadFileName)));
                var contentType = "text/turtle";
                var fileName = payloadFileName;
                return new ContentFile(content, contentType, fileName);
            }
            else
            {
                return null;
            }
        }

        #region Helpers

        private bool ContainerMetadataExists(string id)
        {
            return _context.ContainerMetadata.Any(e => e.Id == id);
        }
        private bool ContainerMetadataVersionExists(string id, string version)
        {
            return _context.ContainerMetadata.Any(e => e.Id == id && e.Version == version);
        }
        private bool isOntologyOrTriple(string filePath)
        {
            var exists = File.Exists(filePath);
            var ext = Path.GetExtension(filePath).ToLower();
            return exists && (ext == ".rdf" || ext == ".ttl" || ext == ".nt" || ext == ".nq" || ext == ".n3" || ext == ".trig");
        }
        private bool ContentsExist(List<CtDocument> documents)
        {
            var result = false;
            foreach (var document in documents)
            {
                result = _context.ContentMetadata.Any(e => e.Id == document.Guid);
            }
            return result;
        }
        private async Task<InformationContainer> ContainerUpdate(InformationContainer container, ContainerMetadata updateMetadata)
        {
            var user = await _userManager.FindByNameAsync(updateMetadata.Modifier);
            var containerModifier = (CtPerson)container.ContainerDescription.ContainsParty.FirstOrDefault(c => c.Name == updateMetadata.Modifier && c.GetType() == typeof(CtPerson));
            if (containerModifier != null)
            {
                if (containerModifier.Description != user.Description)
                {
                    container.ContainerDescription.GetPersonById(containerModifier.Guid).Description = user.Description;
                }
                container.ContainerDescription.Modifier = containerModifier;
            }
            else
            {
                var newPerson = new CtPerson(container, updateMetadata.Modifier, user.Description);
                container.ContainerDescription.Modifier = newPerson;
                container.ContainerDescription.AddPerson(newPerson);
            }

            if (user.Organisation != null && updateMetadata.Status == ContainerStatus.PUBLISHED)
            {
                var containerPublisher = (CtOrganisation)container.ContainerDescription.ContainsParty.SingleOrDefault(c => c.Name == user.Organisation && c.GetType() == typeof(CtOrganisation));
                if (containerPublisher != null)
                {
                    container.ContainerDescription.Publisher = containerPublisher;
                }
                else
                {
                    var newOrganisation = new CtOrganisation(container, user.Organisation, $"Organisation of {user.UserName}");
                    container.ContainerDescription.Publisher = newOrganisation;
                    container.ContainerDescription.AddOrganisation(newOrganisation);
                }
            }

            container.ContainerName = updateMetadata.Name;
            container.ContainerDescription.Description = updateMetadata.Description;
            container.ContainerDescription.IsModified = true;
            container.ContainerDescription.Modification = DateTime.Now;
            return container;
        }

        #endregion
    }
}