using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.Authentication;
using IcddWebApp.Services.Models.Enums;
using IcddWebApp.Services.Models.Requests;
using IIB.ICDD.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace IcddWebApp.Services
{
    public interface IContainerService
    {
        Task<IEnumerable<ContainerMetadata>> GetContainers(string projectId, ContainerType containerType);
        Task<InformationContainer> GetContainer(string projectId, ContainerType containerType, string containerId, string containerVersion);
        Task<ContainerMetadata> GetContainerMetadata(string projectId, ContainerType containerType, string containerId, string containerVersion);
        Task<InformationContainer> PostContainer(string username, string projectId, ContainerType containerType, ContainerMetadataFileRequest request);
        Task<InformationContainer> PostEmptyContainer(string username, string projectId, ContainerType containerType, ContainerMetadataRequest? request);
        Task<IEnumerable<string>> GetContainerVersions(string projectId, ContainerType containerType, string containerId);
        Task<InformationContainer> PostContainerVersion(string username, string projectId, ContainerType containerType, string containerId, ContainerMetadataRequest? request);
        Task<ContainerFile> GetContainerAsFile(string projectId, ContainerType containerType, string containerId, string containerVersion);
        Task<bool> DeleteContainer(string username, string projectId, ContainerType containerType, string containerId, string containerVersion);
        Task<ContainerMetadata> UpdateContainerRecipients(string username, string projectId, string containerId, string containerVersion, List<User> newRecipients);
        Task<bool> AddParticipantToContainer(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, int type, string partName, string partDesc);
        Task<bool> PostOntologyToContainer(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string? webUrl, IFormFile? uploadFile);
        Task<bool> PostPayloadTriplesToContainer(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string? webUrl, IFormFile? uploadFile);
        Task<ContainerMetadata> UpdateContainer(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, ContainerMetadata updateMetadata);
        Task<InformationContainer> ReadWorkfolderContainer(string projectId, string containerId, ContainerType containerType, string containerVersion);
        Task<ContentFile> GetContainerPayloadTriplesAsFile(string projectId, ContainerType containerType, string containerId, string containerVersion, string payloadFileName);

    }
}
