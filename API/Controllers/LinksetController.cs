using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Data;
using IcddWebApp.Services;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.DTOs;
using IcddWebApp.Services.Models.Enums;
using IIB.ICDD.Model.Linkset.Link;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace IcddWebApp.API.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("api/v{apiVersion:apiVersion}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LinksetController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILinksetService _linksetService;

        public LinksetController(DatabaseContext context, ILinksetService linksetService)
        {
            _context = context;
            _linksetService = linksetService;
        }

        /// <summary>
        /// Gets all linksets of newest version of a container
        /// </summary>
        /// <response code="200">Returns linksets as json-list</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [EnableQuery]
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/linksets")]
        public async Task<ActionResult<IEnumerable<LinksetMetadata>>> GetContainerLinksets(string projectId, ContainerType containerType, string containerId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var linksets = await _linksetService.GetContainerLinksets(projectId, containerType, containerId, null);

            if (linksets != null)
                return Ok(linksets);
            else
                return NotFound("Could not find container");
        }

        /// <summary>
        /// Gets all linksets of a specified version of a container
        /// </summary>
        /// <response code="200">Returns linksets as json-list</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [EnableQuery]
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/linksets")]
        public async Task<ActionResult<IEnumerable<LinksetMetadata>>> GetContainerVersionLinksets(string projectId, ContainerType containerType, string containerId, string containerVersion)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var versionLinksets = await _linksetService.GetContainerLinksets(projectId, containerType, containerId, containerVersion);

            if (versionLinksets != null)
                return Ok(versionLinksets);
            else
                return NotFound("Could not find container");
        }

        /// <summary>
        /// Gets linkset of newest version of a container
        /// </summary>
        /// <response code="200">Returns linksets as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/linksets/{linksetId}")]
        public async Task<IActionResult> GetContainerLinkset(string projectId, ContainerType containerType, string containerId, string linksetId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var linkset = await _linksetService.GetContainerLinkset(projectId, containerType, containerId, null, linksetId);

            if (linkset != null)
                return Ok(linkset.ToJsonLD());
            else
                return NotFound("Could not find container or linkset in container");
        }

        /// <summary>
        /// Gets linkset of specified version of a container
        /// </summary>
        /// <response code="200">Returns linksets as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/linksets/{linksetId}")]
        public async Task<IActionResult> GetContainerVersionLinkset(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var linkset = await _linksetService.GetContainerLinkset(projectId, containerType, containerId, containerVersion, linksetId);

            if (linkset != null)
                return Ok(linkset.ToJsonLD());
            else
                return NotFound("Could not find container or linkset in container");
        }

        /// <summary>
        /// Posts linkset in newest version of a container
        /// </summary>
        /// <response code="200">Posts and returns linksets as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/linksets")]
        public async Task<IActionResult> PostContainerLinkset(string projectId, ContainerType containerType, string containerId, string fileName)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newLinkset = await _linksetService.PostContainerLinkset(username, projectId, containerType, containerId, null, fileName);

            if (newLinkset != null)
                return CreatedAtAction("GetContainerLinkset", new { projectId, containerType, containerId, linksetId = newLinkset.Guid }, newLinkset.ToJsonLD());
            else
                return BadRequest("Could not find container or create linkset");
        }

        /// <summary>
        /// Posts linkset in specified version of a container
        /// </summary>
        /// <response code="200">Posts and returns linksets as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/linksets")]
        public async Task<IActionResult> PostContainerVersionLinkset(string projectId, ContainerType containerType, string containerId, string containerVersion, string fileName)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newLinkset = await _linksetService.PostContainerLinkset(username, projectId, containerType, containerId, containerVersion, fileName);

            if (newLinkset != null)
                return CreatedAtAction("GetContainerVersionLinkset", new { projectId, containerType, containerId, containerVersion, linksetId = newLinkset.Guid }, newLinkset.ToJsonLD());
            else
                return BadRequest("Could not find container or create linkset");
        }

        /// <summary>
        /// Gets link of specified linkset of newest version of a container
        /// </summary>
        /// <response code="200">Return links as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/linksets/{linksetId}/links")]
        public async Task<IActionResult> GetContainerLinksetLinks(string projectId, ContainerType containerType, string containerId, string linksetId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var linksetLinks = await _linksetService.GetContainerLinksetLinks(projectId, containerType, containerId, null, linksetId);

            if (linksetLinks != null)
                return Ok(new LinksetLinksDTO(linksetLinks));
            else
                return NotFound("Could not find container or linkset");
        }

        /// <summary>
        /// Gets link of specified linkset of a container
        /// </summary>
        /// <response code="200">Return links as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/linksets/{linksetId}/links")]
        public async Task<IActionResult> GetContainerVersionLinksetLinks(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var linksetLinks = await _linksetService.GetContainerLinksetLinks(projectId, containerType, containerId, containerVersion, linksetId);

            if (linksetLinks != null)
                return Ok(new LinksetLinksDTO(linksetLinks));
            else
                return NotFound("Could not find container or linkset");
        }

        /// <summary>
        /// Posts link of specified linkset of newest version of a container
        /// </summary>
        /// <response code="200">Posts and returns new link as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="request"><b>Identifier types:</b> <br/> No Identifier - 0 <br/> StringBasedIdentifier - 1 <br/> UriBasedIdentifier - 2 <br/> QueryBasedIdentifier - 3 </param>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/linksets/{linksetId}/links/binary")]
        public async Task<IActionResult> PostContainerLinksetBinaryLink(string projectId, ContainerType containerType, string containerId, string linksetId, List<BinaryLinkDTO> request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");
            if (request == null || request.Count < 1)
                return BadRequest("Link Elements cannot be empty");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;

            var newLinks = new List<JObject>();

            if (request.Count == 1)
            {
                BinaryLinkDTO link = request.First();
                if (link.leftElement.hasDocument == null || link.rightElement.hasDocument == null)
                {
                    return BadRequest("Link document id cannot be empty");
                }
                var newLink = await _linksetService.PostContainerLinksetBinaryLink(username, projectId, containerType, containerId, null, linksetId, link);
                newLinks.Add(newLink.ToJsonLD());

            }
            else
            {
                if (request.Any(link => link.leftElement.hasDocument == null || link.rightElement.hasDocument == null))
                    return BadRequest("Link document id cannot be empty");

                var results = await _linksetService.PostContainerLinksetBinaryLinkList(username, projectId, containerType, containerId, null, linksetId, request);
                newLinks.AddRange(results.Select(m => m.ToJsonLD()));
            }
            return Ok(newLinks);
        }

        /// <summary>
        /// Posts link of specified linkset of a container
        /// </summary>
        /// <response code="200">Posts and returns new link as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="request"><b>Identifier types:</b> <br/> No Identifier - 0 <br/> StringBasedIdentifier - 1 <br/> UriBasedIdentifier - 2 <br/> QueryBasedIdentifier - 3 </param>

        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/linksets/{linksetId}/links/binary")]
        public async Task<IActionResult> PostContainerVersionLinksetBinaryLink(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, List<BinaryLinkDTO> request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");
            if (request == null || request.Count.Equals(0))
                return BadRequest("Link Elements cannot be empty");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newLinks = new List<JObject>();
            foreach (var link in request)
            {
                if (link.leftElement.hasDocument == null || link.rightElement.hasDocument == null)
                {
                    return BadRequest("Link document id cannot be empty");
                }
                var newLink = await _linksetService.PostContainerLinksetBinaryLink(username, projectId, containerType, containerId, containerVersion, linksetId, link);
                newLinks.Add(newLink.ToJsonLD());
            }

            if (newLinks != null)
                return Ok(newLinks);
            else
                return NotFound("Could not find container or linkset");
        }

        /// <summary>
        /// Posts link of specified linkset of newest version of a container
        /// </summary>
        /// <response code="200">Posts and returns new link as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="request"><b>Identifier types:</b> <br/> No Identifier - 0 <br/> StringBasedIdentifier - 1 <br/> UriBasedIdentifier - 2 <br/> QueryBasedIdentifier - 3 </param>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/linksets/{linksetId}/links/directedbinary")]
        public async Task<IActionResult> PostContainerLinksetDirectedBinaryLink(string projectId, ContainerType containerType, string containerId, string linksetId, List<DirectedBinaryLinkDTO> request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");
            if (request == null || request.Count.Equals(0))
                return BadRequest("Link Elements cannot be empty");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newLinks = new List<JObject>();
            foreach (var link in request)
            {
                if (link.leftElement.hasDocument == null || link.rightElement.hasDocument == null)
                {
                    return BadRequest("Link document id cannot be empty");
                }
                var newLink = await _linksetService.PostContainerLinksetDirectedBinaryLink(username, projectId, containerType, containerId, null, linksetId, link);
                newLinks.Add(newLink.ToJsonLD());
            }

            if (newLinks != null)
                return Ok(newLinks);
            else
                return NotFound("Could not find container or linkset");
        }

        /// <summary>
        /// Posts link of specified linkset of a container
        /// </summary>
        /// <response code="200">Posts and returns new link as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="request"><b>Identifier types:</b> <br/> No Identifier - 0 <br/> StringBasedIdentifier - 1 <br/> UriBasedIdentifier - 2 <br/> QueryBasedIdentifier - 3 </param>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/linksets/{linksetId}/links/directedbinary")]
        public async Task<IActionResult> PostContainerVersionLinksetDirectedBinaryLink(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, List<DirectedBinaryLinkDTO> request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");
            if (request == null || request.Count.Equals(0))
                return BadRequest("Link Elements cannot be empty");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newLinks = new List<JObject>();
            foreach (var link in request)
            {
                if (link.leftElement.hasDocument == null || link.rightElement.hasDocument == null)
                {
                    return BadRequest("Link document id cannot be empty");
                }
                var newLink = await _linksetService.PostContainerLinksetDirectedBinaryLink(username, projectId, containerType, containerId, containerVersion, linksetId, link);
                newLinks.Add(newLink.ToJsonLD());
            }

            if (newLinks != null)
                return Ok(newLinks);
            else
                return NotFound("Could not find container or linkset");
        }

        /// <summary>
        /// Posts link of specified linkset of newest version of a container
        /// </summary>
        /// <response code="200">Posts and returns new link as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="request"><b>Identifier types:</b> <br/> No Identifier - 0 <br/> StringBasedIdentifier - 1 <br/> UriBasedIdentifier - 2 <br/> QueryBasedIdentifier - 3 </param>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/linksets/{linksetId}/links/directed")]
        public async Task<IActionResult> PostContainerLinksetDirectedLink(string projectId, ContainerType containerType, string containerId, string linksetId, List<DirectedLinkDTO> request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");
            if (request == null || request.Count.Equals(0))
                return BadRequest("Link Elements cannot be empty");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newLinks = new List<JObject>();
            foreach (var link in request)
            {
                foreach (var elem in link.leftElements)
                {
                    if (elem.hasDocument == null)
                    {
                        return BadRequest("Link document id cannot be empty");
                    }
                }
                foreach (var elem in link.rightElements)
                {
                    if (elem.hasDocument == null)
                    {
                        return BadRequest("Link document id cannot be empty");
                    }
                }
                var newLink = await _linksetService.PostContainerLinksetDirectedLink(username, projectId, containerType, containerId, null, linksetId, link);
                newLinks.Add(newLink.ToJsonLD());
            }

            if (newLinks != null)
                return Ok(newLinks);
            else
                return NotFound("Could not find container or linkset");
        }

        /// <summary>
        /// Posts link of specified linkset of a container
        /// </summary>
        /// <response code="200">Posts and returns new link as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="request"><b>Identifier types:</b> <br/> No Identifier - 0 <br/> StringBasedIdentifier - 1 <br/> UriBasedIdentifier - 2 <br/> QueryBasedIdentifier - 3 </param>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/linksets/{linksetId}/links/directed")]
        public async Task<IActionResult> PostContainerVersionLinksetDirectedLink(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, List<DirectedLinkDTO> request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");
            if (request == null || request.Count.Equals(0))
                return BadRequest("Link Elements cannot be empty");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newLinks = new List<JObject>();
            foreach (var link in request)
            {
                foreach (var elem in link.leftElements)
                {
                    if (elem.hasDocument == null)
                    {
                        return BadRequest("Link document id cannot be empty");
                    }
                }
                foreach (var elem in link.rightElements)
                {
                    if (elem.hasDocument == null)
                    {
                        return BadRequest("Link document id cannot be empty");
                    }
                }
                var newLink = await _linksetService.PostContainerLinksetDirectedLink(username, projectId, containerType, containerId, containerVersion, linksetId, link);
                newLinks.Add(newLink.ToJsonLD());
            }

            if (newLinks != null)
                return Ok(newLinks);
            else
                return NotFound("Could not find container or linkset");
        }

        /// <summary>
        /// Posts link of specified linkset of newest version of a container
        /// </summary>
        /// <response code="200">Posts and returns new link as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="request"><b>Identifier types:</b> <br/> No Identifier - 0 <br/> StringBasedIdentifier - 1 <br/> UriBasedIdentifier - 2 <br/> QueryBasedIdentifier - 3 </param>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/linksets/{linksetId}/links/directed1ton")]
        public async Task<IActionResult> PostContainerLinksetDirected1ToNLink(string projectId, ContainerType containerType, string containerId, string linksetId, List<Directed1ToNLinkDTO> request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");
            if (request == null || request.Count.Equals(0))
                return BadRequest("Link Elements cannot be empty");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newLinks = new List<JObject>();
            foreach (var link in request)
            {
                if (link.leftElement.hasDocument == null)
                {
                    return BadRequest("Link document id cannot be empty");
                }

                foreach (var elem in link.rightElements)
                {
                    if (elem.hasDocument == null)
                    {
                        return BadRequest("Link document id cannot be empty");
                    }
                }
                var newLink = await _linksetService.PostContainerLinksetDirected1ToNLink(username, projectId, containerType, containerId, null, linksetId, link);
                newLinks.Add(newLink.ToJsonLD());
            }

            if (newLinks != null)
                return Ok(newLinks);
            else
                return NotFound("Could not find container or linkset");
        }

        /// <summary>
        /// Posts link of specified linkset of a container
        /// </summary>
        /// <response code="200">Posts and returns new link as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="request"><b>Identifier types:</b> <br/> No Identifier - 0 <br/> StringBasedIdentifier - 1 <br/> UriBasedIdentifier - 2 <br/> QueryBasedIdentifier - 3 </param>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/linksets/{linksetId}/links/directed1ton")]
        public async Task<IActionResult> PostContainerVersionLinksetDirected1ToNLink(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, List<Directed1ToNLinkDTO> request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");
            if (request == null || request.Count.Equals(0))
                return BadRequest("Link Elements cannot be empty");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newLinks = new List<JObject>();
            foreach (var link in request)
            {
                if (link.leftElement.hasDocument == null)
                {
                    return BadRequest("Link document id cannot be empty");
                }

                foreach (var elem in link.rightElements)
                {
                    if (elem.hasDocument == null)
                    {
                        return BadRequest("Link document id cannot be empty");
                    }
                }
                var newLink = await _linksetService.PostContainerLinksetDirected1ToNLink(username, projectId, containerType, containerId, containerVersion, linksetId, link);
                newLinks.Add(newLink.ToJsonLD());
            }

            if (newLinks != null)
                return Ok(newLinks);
            else
                return NotFound("Could not find container or linkset");
        }

        /// <summary>
        /// Deletes linkset in container
        /// </summary>
        /// <response code="200">Deletes linkset</response>
        /// <response code="400">Could not delete linkset</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpDelete("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/linksets/{linksetId}")]
        public async Task<IActionResult> DeleteContainerVersionLinkset(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var result = await _linksetService.DeleteContainerLinkset(projectId, containerType, containerId, containerVersion, linksetId);

            if (result)
                return Ok("Linkset " + linksetId + " has been deleted");
            else
                return BadRequest("Could not delete linkset");
        }

        /// <summary>
        /// Deletes linkset link in container
        /// </summary>
        /// <response code="200">Deletes linkset link</response>
        /// <response code="400">Could not delete linkset link</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpDelete("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/linksets")]
        public async Task<IActionResult> DeleteContainerVersionLinksetLink(string projectId, ContainerType containerType, string containerId, string containerVersion, string linksetId, string linkId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var result = await _linksetService.DeleteContainerLinksetLink(projectId, containerType, containerId, containerVersion, linksetId, linkId);

            if (result)
                return Ok("Link" + linkId + " has been deleted");
            else
                return BadRequest("Could not delete link");
        }


        #region Helpers

        private bool ProjectExists(string id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        #endregion
    }
}
