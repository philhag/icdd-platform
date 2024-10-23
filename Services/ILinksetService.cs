using IcddWebApp.Services.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.Enums;
using IIB.ICDD.Model.Container;
using IIB.ICDD.Model.Linkset.Link;
using IcddWebApp.Services.Models.Update;

namespace IcddWebApp.Services
{
    public interface ILinksetService
    {
        Task<IEnumerable<LinksetMetadata>> GetContainerLinksets(string projectId, ContainerType containerType, string containerId, string containerVersion);
        Task<CtLinkset> GetContainerLinkset(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId);
        Task<LinksetMetadata> GetContainerLinksetMetadata(string linksetId, string containerInternalId);
        Task<CtLinkset> PostContainerLinkset(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string fileName);
        Task<IEnumerable<LsLink>> GetContainerLinksetLinks(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId);
        Task<LsBinaryLink> PostContainerLinksetBinaryLink(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, BinaryLinkDTO linkElementDTO);
        Task<List<LsBinaryLink>> PostContainerLinksetBinaryLinkList(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, List<BinaryLinkDTO> linkElementDTO);
        Task<LsDirectedLink> PostContainerLinksetDirectedLink(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, DirectedLinkDTO linkElementDTO);
        Task<LsDirectedBinaryLink> PostContainerLinksetDirectedBinaryLink(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, DirectedBinaryLinkDTO linkElementDTO);
        Task<LsDirected1ToNLink> PostContainerLinksetDirected1ToNLink(string username, string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, Directed1ToNLinkDTO linkElementDTO);
        Task<LinksetMetadata> UpdateContainerLinkset(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, string modifier, LinksetMetadataUpdate updateMetadata);        Task<bool> DeleteContainerLinkset(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId);
        Task<bool> DeleteContainerLinksetLink(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, string linkId);
        //Task<ContentFile> GetContainerLinksetAsFile(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId);
    }
}
