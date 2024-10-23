using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.Enums;
using IcddWebApp.Services.Models.Requests;
using IcddWebApp.Services.Models.Update;
using IIB.ICDD.Model.Container.Document;
using Microsoft.AspNetCore.Http;

namespace IcddWebApp.Services
{
    public interface IContentService
    {
        Task<IEnumerable<ContentMetadata>> GetContainerContents(string projectId, ContainerType containerType, string containerId, string containerVersion);
        Task<CtDocument> PostContainerContent(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, DocumentRequest request);
        Task<CtDocument> PostContainerContentInternal(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, InternalDocumentRequest request);
        Task<CtDocument> PostContainerContentExternal(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, ExternalDocumentRequest request);
        Task<CtDocument> PostContainerContentDatabase(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, DatabaseRequest request);
        Task<CtDocument> PostContainerContentEncrypted(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, EncryptedDocumentRequest request);
        Task<CtDocument> PostContainerContentFolder(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, FolderDocumentRequest request);
        Task<CtDocument> PostContainerContentSecured(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, SecuredDocumentRequest request);
        Task<CtDocument> GetContainerContent(string projectId, ContainerType containerType, string containerId, string containerVersion, string contentId);
        Task<ContentMetadata> GetContainerContentMetadata(string contentId, string containerInternalId);
        Task<ContentMetadata> UpdateContainerContent(string projectId, ContainerType containerType, string containerId, string containerVersion, string contentId, string modifier, ContentMetadataUpdate updateMetadata);
        Task<ContentFile> GetContainerContentAsFile(string projectId, ContainerType containerType, string containerId, string containerVersion, string contentId);
        Task<bool> DeleteContent(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string contentId);
        Task<CtDocument> AddRequestedDocument(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string contentId, IFormFile uploadFile, bool changeType, string newFileExt, string newMimeType);
    }
}
