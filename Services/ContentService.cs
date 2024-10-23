using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO.Compression;
using IcddWebApp.Data;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.Enums;
using IcddWebApp.Services.Models.Requests;
using IIB.ICDD.Model;
using IIB.ICDD.Model.Container;
using IIB.ICDD.Model.Container.Document;
using IIB.ICDD.Model.Interfaces;
using IIB.ICDD.Handling;
using IIB.ICDD.Parsing;
using IcddWebApp.Services.Models.Update;
using IIB.ICDD.Model.Container.ExtendedDocument;
using Microsoft.AspNetCore.Http;

namespace IcddWebApp.Services
{
    public class ContentService : IContentService
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _workfolderPath;
        private readonly IContainerService _containerService;

        public ContentService(DatabaseContext context, IConfiguration configuration, IContainerService containerService)
        {
            _context = context;
            _configuration = configuration;
            _workfolderPath = _configuration["WorkfolderPath"];
            _containerService = containerService;
        }

        public async Task<IEnumerable<ContentMetadata>> GetContainerContents(string projectId, ContainerType containerType, string containerId, string containerVersion)
        {
            ContainerMetadata containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            return containerMetadata.Content.ToList();
        }

        public async Task<CtDocument> PostContainerContentInternal(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, InternalDocumentRequest request)
        {
            ContainerMetadata containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);
            var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
            var container = reader.Read();

            CtDocument newDocument = null;

            var tempInternalFilePath = Path.Combine(projectPath, request.File.FileName);
            using (var fs = System.IO.File.Create(tempInternalFilePath))
            {
                await request.File.CopyToAsync(fs);
                fs.Flush();
            }
            newDocument = container.CreateInternalDocument(tempInternalFilePath, request.File.FileName, request.File.FileName.Split('.').Last(), request.File.ContentType);
            System.IO.File.Delete(tempInternalFilePath);

            if (newDocument != null)
            {
                if (request.Description != null)
                    newDocument.Description = request.Description;

                var creator = container.ContainerDescription.ContainsParty.Where(x => x.Name == username && x.GetType() == typeof(CtPerson)).SingleOrDefault();
                if (creator != null)
                    newDocument.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
                else
                    newDocument.Creator = new CtPerson(container, username, $"description for {username}");
                containerMetadata.Content.Add(new ContentMetadata(newDocument, containerMetadata.InternalId, Path.Combine(projectPath, containerMetadata.InternalId, "payload documents", request.File.FileName), request.Schema, request.SchemaVersion, request.SchemaSubset));

                container.SaveRdf();
                container.Repository.Commit("Container content added " + request.File.FileName, username);
                var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                writer.Write(container);

                await _context.SaveChangesAsync();
                return newDocument;
            }
            else
            {
                return null;
            }

        }

        public async Task<CtDocument> PostContainerContent(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, DocumentRequest request)
        {
            ContainerMetadata containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);
            var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
            var container = reader.Read();

            CtDocument newDocument = container.CreateInternalDocument(request.Filename, request.FileExtension?.Split('.').Last(), request.MimeType);

            if (newDocument != null)
            {
                if (request.Description != null)
                    newDocument.Description = request.Description;

                var creator = container.ContainerDescription.ContainsParty.SingleOrDefault(x => x.Name == username && x.GetType() == typeof(CtPerson));
                if (creator != null)
                    newDocument.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
                else
                    newDocument.Creator = new CtPerson(container, username, $"description for {username}");
                containerMetadata.Content.Add(new ContentMetadata(newDocument, containerMetadata.InternalId, Path.Combine(projectPath, containerMetadata.InternalId, "payload documents", request.Filename), request.Schema, request.SchemaVersion, request.SchemaSubset));

                container.SaveRdf();
                container.Repository.Commit("Container content added " + request.Filename, username);
                var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                writer.Write(container);

                await _context.SaveChangesAsync();
                return newDocument;
            }
            else
            {
                return null;
            }

        }

        public async Task<CtDocument> PostContainerContentExternal(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, ExternalDocumentRequest request)
        {
            ContainerMetadata containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);

            var projectPath = Path.Combine(_workfolderPath, projectId);

            var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
            var container = reader.Read();

            CtDocument newDocument = null;
            newDocument = container.CreateExternalDocument(request.DocumentUri, request.FileName, request.FileExtension, request.MimeType);
            if (request.Description != null)
                newDocument.Description = request.Description;

            var creator = container.ContainerDescription.ContainsParty.Where(x => x.Name == username && x.GetType() == typeof(CtPerson)).SingleOrDefault();
            if (creator != null)
                newDocument.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
            else
                newDocument.Creator = new CtPerson(container, username, $"description for {username}");
            containerMetadata.Content.Add(new ContentMetadata(newDocument, containerMetadata.InternalId, request.DocumentUri, request.Schema, request.SchemaVersion, request.SchemaSubset));

            if (newDocument != null)
            {
                container.SaveRdf();
                container.Repository.Commit("External container content added " + request.FileName, username);
                var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                writer.Write(container);

                await _context.SaveChangesAsync();
                return newDocument;
            }
            else
            {
                return null;
            }
        }

        public async Task<CtDocument> PostContainerContentDatabase(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, DatabaseRequest request)
        {
            ContainerMetadata containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);
            var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
            var container = reader.Read();

            CtDocument mappingFile = null;
            if (!string.IsNullOrEmpty(request.InternalMappingDocument))
                mappingFile = container.GetDocument(new Uri(request.InternalMappingDocument).Fragment);

            CtDocument newDocument = null;

            newDocument = container.CreateDatabaseLink(request.ConnectionString, request.DatabaseName, request.DatabaseType, request.QueryLanguage, mappingFile);

            if (newDocument != null)
            {
                var creator = container.ContainerDescription.ContainsParty.SingleOrDefault(x => x.Name == username && x.GetType() == typeof(CtPerson));
                if (creator != null)
                    newDocument.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
                else
                    newDocument.Creator = new CtPerson(container, username, $"description for {username}");
                containerMetadata.Content.Add(new ContentMetadata(newDocument, containerMetadata.InternalId, request.ConnectionString, request.Schema, request.SchemaVersion, request.SchemaSubset));


                container.SaveRdf();
                container.Repository.Commit("Database added " + request.DatabaseName, username);
                var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), false);
                writer.Write(container);

                await _context.SaveChangesAsync();
                return newDocument;
            }
            else
            {
                return null;
            }
        }

        public async Task<CtDocument> PostContainerContentEncrypted(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, EncryptedDocumentRequest request)
        {
            ContainerMetadata containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);
            var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
            var container = reader.Read();

            CtDocument newDocument = null;

            var tempEncryptedFilePath = Path.Combine(projectPath, request.File.FileName);
            using (var fs = System.IO.File.Create(tempEncryptedFilePath))
            {
                await request.File.CopyToAsync(fs);
                fs.Flush();
            }

            newDocument = container.CreateEncryptedDocument(tempEncryptedFilePath, request.File.FileName, request.File.FileName.Split('.').Last(), request.File.ContentType, request.EncryptionAlgorithm);
            System.IO.File.Delete(tempEncryptedFilePath);
            var creator = container.ContainerDescription.ContainsParty.Where(x => x.Name == username && x.GetType() == typeof(CtPerson)).SingleOrDefault();
            if (creator != null)
                newDocument.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
            else
                newDocument.Creator = new CtPerson(container, username, $"description for {username}");
            containerMetadata.Content.Add(new ContentMetadata(newDocument, containerMetadata.InternalId, Path.Combine(projectPath, containerMetadata.InternalId, "payload documents", request.File.FileName), request.Schema, request.SchemaVersion, request.SchemaSubset));

            if (newDocument != null)
            {
                container.SaveRdf();
                container.Repository.Commit("Encrypted content added " + request.File.FileName, username);
                var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                writer.Write(container);

                await _context.SaveChangesAsync();
                return newDocument;
            }
            else
            {
                return null;
            }
        }

        public async Task<CtDocument> PostContainerContentFolder(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, FolderDocumentRequest request)
        {
            ContainerMetadata containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);
            var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
            var container = reader.Read();

            CtDocument newDocument = null;

            newDocument = container.CreateFolderDocument(request.FolderName);
            var creator = container.ContainerDescription.ContainsParty.Where(x => x.Name == username && x.GetType() == typeof(CtPerson)).SingleOrDefault();
            if (creator != null)
                newDocument.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
            else
                newDocument.Creator = new CtPerson(container, username, $"description for {username}");
            containerMetadata.Content.Add(new ContentMetadata(newDocument, containerMetadata.InternalId, Path.Combine(projectPath, containerMetadata.InternalId, "payload documents", request.FolderName), request.Schema, request.SchemaVersion, request.SchemaSubset)); // TODO: fuer spaeter location updaten

            if (newDocument != null)
            {
                container.SaveRdf();
                container.Repository.Commit("Folder added " + request.FolderName, username);
                var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                writer.Write(container);

                await _context.SaveChangesAsync();
                return newDocument;
            }
            else
            {
                return null;
            }
        }

        public async Task<CtDocument> PostContainerContentSecured(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, SecuredDocumentRequest request)
        {
            ContainerMetadata containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);
            var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
            var container = reader.Read();

            CtDocument newDocument = null;

            var tempSecuredFilePath = Path.Combine(projectPath, request.File.FileName);
            using (var fs = System.IO.File.Create(tempSecuredFilePath))
            {
                await request.File.CopyToAsync(fs);
                fs.Flush();
            }
            newDocument = container.CreateSecuredDocument(tempSecuredFilePath, request.File.FileName, request.File.FileName.Split('.').Last(), request.File.ContentType, request.Checksum, request.ChecksumAlgorithm);
            System.IO.File.Delete(tempSecuredFilePath);
            var creator = container.ContainerDescription.ContainsParty.Where(x => x.Name == username && x.GetType() == typeof(CtPerson)).SingleOrDefault();
            if (creator != null)
                newDocument.Creator = container.ContainerDescription.GetPersonById(creator.Guid);
            else
                newDocument.Creator = new CtPerson(container, username, $"description for {username}");
            containerMetadata.Content.Add(new ContentMetadata(newDocument, containerMetadata.InternalId, Path.Combine(projectPath, containerMetadata.InternalId, "payload documents", request.File.FileName), request.Schema, request.SchemaVersion, request.SchemaSubset));

            if (newDocument != null)
            {
                container.SaveRdf();
                container.Repository.Commit("Secured content added " + request.File.FileName, username);
                var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                writer.Write(container);

                await _context.SaveChangesAsync();
                return newDocument;
            }
            else
            {
                return null;
            }
        }

        public async Task<CtDocument> GetContainerContent(string projectId, ContainerType containerType, string containerId, string containerVersion, string contentId)
        {
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);
            var reader = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd");
            var container = reader.Read();
            var content = container.GetDocument(contentId);
            return content;
        }

        public async Task<ContentMetadata> GetContainerContentMetadata(string contentId, string containerInternalId)
        {
            return await _context.ContentMetadata.Where(x => x.Id == contentId && x.ContainerInternalId == containerInternalId).SingleOrDefaultAsync(); ;
        }

        public async Task<ContentMetadata> UpdateContainerContent(string projectId, ContainerType containerType, string containerId, string containerVersion, string contentId, string modifier, ContentMetadataUpdate updateMetadata)
        {
            var container = await _containerService.GetContainer(projectId, containerType, containerId, containerVersion);
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var contentMetadata = await GetContainerContentMetadata(contentId, containerMetadata.InternalId);

            var content = container.GetDocument(contentId);

            if (container != null && contentMetadata != null && content != null)
            {
                contentMetadata.Name = updateMetadata.Name;
                contentMetadata.Location = Path.Combine(container.GetDocumentFolder(), updateMetadata.Name);

                if (content.IsSameOrSubclass(typeof(CtInternalDocument)))
                {
                    var internalContent = content as CtInternalDocument;
                    internalContent.FileName = updateMetadata.Name;
                }
                else
                {
                    content.Name = updateMetadata.Name;
                }

                if (content.GetType() != typeof(ExtDatabaseLink))
                {
                    contentMetadata.Description = updateMetadata.Description;
                    content.Description = updateMetadata.Description;
                    contentMetadata.Schema = updateMetadata.Schema;
                    contentMetadata.SchemaVersion = updateMetadata.SchemaVersion;
                    contentMetadata.SchemaSubset = updateMetadata.SchemaSubset;
                }

                var containerModifier = (CtPerson)container.ContainerDescription.ContainsParty.SingleOrDefault(c => c.Name == modifier && c.GetType() == typeof(CtPerson));
                if (containerModifier != null)
                {
                    contentMetadata.Modifier = modifier;
                    content.Modifier = containerModifier;
                }
                else
                {
                    contentMetadata.Modifier = modifier;
                    content.Modifier = new CtPerson(container, modifier, $"description for {modifier}");
                }

                contentMetadata.Modified = DateTime.Now;
                content.Modification = DateTime.Now;

                _context.ContentMetadata.Update(contentMetadata);
                _context.Entry(contentMetadata).State = EntityState.Modified;

                if (content.GetType() == typeof(ExtDatabaseLink))
                {
                    var dataBaseContent = (ExtDatabaseLink)content;
                    if (updateMetadata.MappingFile != null)
                        dataBaseContent.DbMapping = container.GetDocument(updateMetadata.MappingFile);
                    else if (dataBaseContent.DbMapping != null)
                        dataBaseContent.DbMapping = null;
                }

                try
                {
                    var meta = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
                    meta.Modifier = modifier;
                    meta.Modified = DateTime.Now;

                    await _containerService.UpdateContainer(modifier, projectId, containerType, containerId, containerVersion, meta);
                    await _context.SaveChangesAsync();
                    container.SaveRdf();
                    container.Repository.Commit("Container updated " + contentMetadata.Name, modifier);
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
                return contentMetadata;
            }
            else
            {
                return null;
            }
        }

        public async Task<ContentFile> GetContainerContentAsFile(string projectId, ContainerType containerType, string containerId, string containerVersion, string contentId)
        {
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var contentMetadata = containerMetadata.Content
                .Where(x => x.Id == contentId)
                .SingleOrDefault();

            var container = await _containerService.GetContainer(projectId, containerType, containerId, containerVersion);
            var document = container.GetDocument(contentId);
            bool isFolder = document.GetType() == typeof(CtFolderDocument);

            if (contentMetadata != null)
            {
                if (isFolder)
                {
                    var files = Directory.GetFiles(contentMetadata.Location);
                    var ms = new MemoryStream();
                    using (ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Update, leaveOpen: true))
                    {
                        foreach (var file in files)
                        {
                            var botFileName = Path.GetFileName(file);
                            var entry = archive.CreateEntry(botFileName);
                            using (var entryStream = entry.Open())
                            using (var fileStream = System.IO.File.OpenRead(file))
                            {
                                await fileStream.CopyToAsync(entryStream);
                            }
                        }
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    return new ContentFile(ms, "application/zip", contentMetadata.Name + ".zip");
                }
                else
                {
                    var content = new MemoryStream(new WebClient().DownloadData(contentMetadata.Location));
                    var contentType = contentMetadata.Type;
                    var fileName = contentMetadata.Name;
                    return new ContentFile(content, contentType, fileName);
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteContent(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string contentId)
        {
            var container = await _containerService.GetContainer(projectId, containerType, containerId, containerVersion);
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var contentMetadata = await GetContainerContentMetadata(contentId, containerMetadata.InternalId);

            if (container != null && contentMetadata != null && container.DeleteDocument(contentId))
            {
                var projectPath = Path.Combine(_workfolderPath, projectId);
                container.SaveRdf();
                container.Repository.Commit("Content deleted " + contentMetadata.Name, username);
                var writer = new IcddContainerWriter(Path.Combine(projectPath, $"{containerMetadata.InternalId}.icdd"), true);
                writer.Write(container);
                _context.Entry(contentMetadata).State = EntityState.Deleted;
                _context.ContentMetadata.Remove(contentMetadata);
                await _context.SaveChangesAsync();

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<CtDocument> AddRequestedDocument(string username, string projectId, ContainerType containerType, string containerId,
            string containerVersion, string contentId, IFormFile uploadFile, bool changeType, string newFileExt, string newMimeType)
        {
            var container = await _containerService.GetContainer(projectId, containerType, containerId, containerVersion);
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var contentMetadata = await GetContainerContentMetadata(contentId, containerMetadata.InternalId);
            var content = container.GetDocument(contentId);

            if (container != null && contentMetadata != null && uploadFile != null && content != null)
            {
                var projectPath = Path.Combine(_workfolderPath, projectId);
                var targetPath = content.AbsoluteFilePath();
                var newFileName = Path.GetFileNameWithoutExtension(content.Name) + Path.GetExtension(uploadFile.FileName);
                if (changeType)
                    targetPath = Path.Combine(container.GetDocumentFolder(), newFileName);

                try
                {
                    using (var fs = System.IO.File.Create(targetPath))
                    {
                        await uploadFile.CopyToAsync(fs);
                        fs.Flush();
                    }
                }
                catch
                {
                    return null;
                }

                contentMetadata.Modified = DateTime.Now;
                var containerModifier = (CtPerson)container.ContainerDescription.ContainsParty.SingleOrDefault(c => c.Name == username && c.GetType() == typeof(CtPerson));
                if (containerModifier != null)
                {
                    contentMetadata.Modifier = username;
                    content.Modifier = containerModifier;
                }
                else
                {
                    contentMetadata.Modifier = username;
                    content.Modifier = new CtPerson(container, username, $"description for {username}");
                }
                content.Requested = false;
                if (changeType && newFileExt != null && newMimeType != null)
                {
                    content.Name = newFileName;
                    content.FileType = newFileExt;
                    content.FileFormat = newMimeType;
                    (content as CtInternalDocument).FileName = newFileName;
                    contentMetadata.Type = newMimeType;
                    contentMetadata.Name = newFileName;
                    contentMetadata.Location = Path.GetFullPath(targetPath);
                }
                var test = content.AbsoluteFilePath();
                 _context.ContentMetadata.Update(contentMetadata);
                _context.Entry(contentMetadata).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                container.SaveRdf();
                container.Repository.Commit("Requested content added " + contentMetadata.Name, username);

                return content;
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
        private bool ContentMetadataExists(string id)
        {
            return _context.ContentMetadata.Any(e => e.Id == id);
        }
        private bool ProjectExists(string id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        #endregion
    }
}
