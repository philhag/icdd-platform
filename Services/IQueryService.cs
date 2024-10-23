using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Services.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace IcddWebApp.Services
{
    public interface IQueryService
    {
        Task<string> GetQueryContainer(string query, string projectId, ContainerType containerType, string containerId, string containerVersion, bool applyInference);
        Task<string> GetShaclContainer(string shapes, string projectId, ContainerType containerType, string containerId, string containerVersion, bool applyInference);
        Task<bool> PostContainerQuery(string projectId, ContainerType containerType, string containerId, string containerVersion, string query, string queryName, bool applyInference);
        Task<bool> DeleteContainerQuery(string projectId, ContainerType containerType, string containerId, string containerVersion, string queryId);

    }
}
