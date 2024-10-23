using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Data;
using IcddWebApp.Services;
using IcddWebApp.Services.Models.Authentication;
using IcddWebApp.Services.Models.DTOs;
using IcddWebApp.Services.Models.Enums;
using IcddWebApp.Services.Models.Requests;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcddWebApp.API.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("api/v{apiVersion:apiVersion}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ContainerController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IContainerService _containerService;
        private readonly IQueryService _queryService;
        private readonly IAuthService _authService;

        public ContainerController(DatabaseContext context, IContainerService containerService, IQueryService queryService, IAuthService authService)
        {
            _context = context;
            _containerService = containerService;
            _queryService = queryService;
            _authService = authService;
        }


        /// <summary>
        /// Returns existing container types
        /// </summary>
        /// <response code="200">Returns types</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes")]
        public ActionResult<IEnumerable<ContainerType>> GetContainerTypes(string projectId)
        {
            if (ProjectExists(projectId))
                return new List<ContainerType> { ContainerType.ICDD };
            else
                return NotFound("The Project does not exist");
        }

        /// <summary>
        /// Returns existing containers in database
        /// </summary>
        /// <response code="200">Returns container-list as json</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [EnableQuery]
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers")]
        public async Task<ActionResult<IEnumerable<ContainerMetadataDTO>>> GetContainers(string projectId, ContainerType containerType)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var containers = await _containerService.GetContainers(projectId, containerType);
            var containerDTOs = new List<ContainerMetadataDTO>();
            foreach (var container in containers)
            {
                containerDTOs.Add(new ContainerMetadataDTO(container));
            }
            return Ok(containerDTOs);
        }

        /// <summary>
        /// Returns container by id
        /// </summary>
        /// <response code="200">Returns container as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}")]
        public async Task<IActionResult> GetContainer(string projectId, ContainerType containerType, string containerId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var container = await _containerService.GetContainer(projectId, containerType, containerId, null);

            if (container != null)
                return Ok(new InformationContainerDTO(container, containerId));
            else
                return NotFound("Could not find container");
        }

        /// <summary>
        /// Query container with SPARQL
        /// </summary>
        /// <response code="200">Returns json</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/query")]
        public async Task<IActionResult> GetQueryContainer(string query, string projectId, ContainerType containerType, string containerId, bool applyInference = false)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var result = await _queryService.GetQueryContainer(query, projectId, containerType, containerId, null, applyInference);

            if (!string.IsNullOrEmpty(result))
                return Content(result, "application/sparql-results+json");
            else
                return NotFound("Could not find or query container");
        }

        /// <summary>
        /// Query container version with SPARQL
        /// </summary>
        /// <response code="200">Returns json</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/query")]
        public async Task<IActionResult> GetQueryContainerVersion(string query, string projectId, ContainerType containerType, string containerId, string containerVersion, bool applyInference)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var result = await _queryService.GetQueryContainer(query, projectId, containerType, containerId, containerVersion, applyInference);

            if (!string.IsNullOrEmpty(result))
                return Content(result, "application/sparql-results+json");
            else
                return NotFound("Could not find or query container");
        }

        /// <summary>
        /// Posts new container (id will be overwritten)
        /// </summary>
        /// <response code="200">Posts and returns new container</response>
        /// <response code="400">Could not read container or file is missing or not of type .icdd</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project does not exist</response>
        /// <response code="409">Could not rename container file</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers")]
        public async Task<IActionResult> PostContainer(string projectId, ContainerType containerType, [FromForm] ContainerMetadataFileRequest request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContainer = await _containerService.PostContainer(username, projectId, containerType, request);

            if (newContainer != null)
                return CreatedAtAction("GetContainer", new { projectId, containerType, containerId = newContainer.ContainerGuid }, new InformationContainerDTO(newContainer));
            else
                return BadRequest("File is missing or not of type .icdd");
        }

        /// <summary>
        /// Posts new empty container 
        /// </summary>
        /// <response code="200">Posts and returns new container</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/empty")]
        public async Task<IActionResult> PostEmptyContainer(string projectId, ContainerType containerType, ContainerMetadataRequest? request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContainer = await _containerService.PostEmptyContainer(username, projectId, containerType, request);

            if (newContainer != null)
                return CreatedAtAction("GetContainer", new { projectId, containerType, containerId = newContainer.ContainerGuid }, new InformationContainerDTO(newContainer));
            else
                return BadRequest("Could not create new Container");
        }

        /// <summary>
        /// Gets all versions of container 
        /// </summary>
        /// <response code="200">Returns existing versions</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [EnableQuery]
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions")]
        public async Task<ActionResult<IEnumerable<string>>> GetContainerVersions(string projectId, ContainerType containerType, string containerId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var containerVersions = await _containerService.GetContainerVersions(projectId, containerType, containerId);

            if (containerVersions != null)
                return Ok(containerVersions);
            else
                return NotFound("Could not find container");
        }

        /// <summary>
        /// Returns specified container version
        /// </summary>
        /// <response code="200">Returns container as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}")]
        public async Task<IActionResult> GetContainerVersion(string projectId, ContainerType containerType, string containerId, string containerVersion)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var container = await _containerService.GetContainer(projectId, containerType, containerId, containerVersion);

            if (containerVersion != null)
                return Ok(new InformationContainerDTO(container, containerId));
            else
                return NotFound("Could not find container");
        }

        /// <summary>
        /// Posts new container version
        /// </summary>
        /// <response code="200">Posts and returns new container as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="409">Container version already exists</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}")]
        public async Task<IActionResult> PostContainerVersion(string projectId, ContainerType containerType, string containerId, ContainerMetadataRequest? request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContainer = await _containerService.PostContainerVersion(username, projectId, containerType, containerId, request);

            if (newContainer != null)
                return CreatedAtAction("GetContainerVersion", new { projectId, containerType, containerId, containerVersion = newContainer.VersionID }, new InformationContainerDTO(newContainer, containerId));
            else
                return BadRequest("Could not find container or container version already exists");
        }


        /// <summary>
        /// Get file of newest container version
        /// </summary>
        /// <response code="200">Returns container as file</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/attachment")]
        public async Task<IActionResult> GetContainerAsFile(string projectId, ContainerType containerType, string containerId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var file = await _containerService.GetContainerAsFile(projectId, containerType, containerId, null);

            if (file != null)
                return File(file.Content, file.ContentType, file.FileName);
            else
                return NotFound("Could not find container");
        }

        /// <summary>
        /// Get file of specified container version
        /// </summary>
        /// <response code="200">Returns container as file</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/attachment")]
        public async Task<IActionResult> GetContainerVersionAsFile(string projectId, ContainerType containerType, string containerId, string containerVersion)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var file = await _containerService.GetContainerAsFile(projectId, containerType, containerId, containerVersion);

            if (file != null)
                return File(file.Content, file.ContentType, file.FileName);
            else
                return NotFound("Could not find container");
        }

        /// <summary>
        /// Deletes newest version of the container
        /// </summary>
        /// <response code="200">Deletes container</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="409">Container version already exists</response>
        /// <response code="500">Internal Server Error</response>
        [HttpDelete("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}")]
        public async Task<IActionResult> DeleteContainer(string projectId, ContainerType containerType, string containerId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var result = await _containerService.DeleteContainer(User.Claims.FirstOrDefault(c => c.Type == "Username").Value, projectId, containerType, containerId, null);

            if (result)
                return Ok("Container " + containerId + " has been deleted");
            else
                return BadRequest("Could not delete container");
        }

        /// <summary>
        /// Deletes specified version of the container
        /// </summary>
        /// <response code="200">Deletes container</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="409">Container version already exists</response>
        /// <response code="500">Internal Server Error</response>
        [HttpDelete("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}")]
        public async Task<IActionResult> DeleteContainerVersion(string projectId, ContainerType containerType, string containerId, string containerVersion)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var result = await _containerService.DeleteContainer(User.Claims.FirstOrDefault(c => c.Type == "Username").Value, projectId, containerType, containerId, containerVersion);

            if (result)
                return Ok("Container-Version " + containerVersion + " of container " + containerId + " has been deleted");
            else
                return BadRequest("Could not delete container");
        }

        ///// <summary>
        ///// add participants to newest container
        ///// </summary>
        ///// <response code="200">adds participants and returns container as json-ld</response>
        ///// <response code="401">User needs to be logged in</response>
        ///// <response code="400">Could not update container recipients</response>
        ///// <response code="404">The Project or container does not exist</response>
        ///// <response code="500">Internal Server Error</response>
        //[HttpPut("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/participants")]
        //public async Task<IActionResult> AddParticipantToContainer(string projectId, ContainerType containerType, string containerId, string containerVersion, int type, string partName, string partDesc)
        //{
        //    if (!ProjectExists(projectId))
        //        return NotFound("The Project does not exist");

        //    var result = await _containerService.AddParticipantToContainer(projectId, containerType, containerId, type, partName, partDesc);

        //    if (result) {
        //        var container = await _containerService.GetContainer(projectId, containerType, containerId);
        //        return Ok(new InformationContainerDTO(container, containerId));
        //    }
        //    else
        //        return BadRequest("Could not update container recipients");
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
        private bool ProjectExists(string id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        #endregion
    }
}
