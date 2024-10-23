using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Data;
using IcddWebApp.Services;
using IcddWebApp.Services.Models;
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
    public class ContentController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IContentService _contentService;

        public ContentController(DatabaseContext context, IContentService contentService)
        {
            _context = context;
            _contentService = contentService;
        }


        /// <summary>
        /// Gets all contents of newest version of a container
        /// </summary>
        /// <response code="200">Returns contents as json-list</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [EnableQuery]
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/contents")]
        public async Task<ActionResult<IEnumerable<ContentMetadata>>> GetContainerContents(string projectId, ContainerType containerType, string containerId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var contents = await _contentService.GetContainerContents(projectId, containerType, containerId, null);

            if (contents != null)
                return Ok(contents);
            else
                return NotFound("Could not find container");
        }

        /// <summary>
        /// Gets all contents of specified version of a container
        /// </summary>
        /// <response code="200">Returns contents as json-list</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [EnableQuery]
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/contents")]
        public async Task<ActionResult<IEnumerable<ContentMetadata>>> GetContainerVersionContents(string projectId, ContainerType containerType, string containerId, string containerVersion)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var versionContents = await _contentService.GetContainerContents(projectId, containerType, containerId, containerVersion);

            if (versionContents != null)
                return Ok(versionContents);
            else
                return NotFound("Could not find container");
        }

        /// <summary>
        /// Posts new internal content to newest version of the container
        /// </summary>
        /// <response code="200">Posts and returns new content as json-ld</response>
        /// <response code="400">Could not create content</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/contents/internal")]
        public async Task<IActionResult> PostContainerContentInternal(string projectId, ContainerType containerType, string containerId, [FromForm] InternalDocumentRequest request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContent = await _contentService.PostContainerContentInternal(username, projectId, containerType, containerId, null, request);

            if (newContent != null)
                return CreatedAtAction("GetContainerContent", new { projectId, containerType, containerId, contentId = newContent.Guid }, newContent.ToJsonLD());
            else
                return BadRequest("Could not find container or create document");
        }

        /// <summary>
        /// Posts new internal content to a specified container version
        /// </summary>
        /// <response code="200">Posts and returns new content as json-ld</response>
        /// <response code="400">Could not create content</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/contents/internal")]
        public async Task<IActionResult> PostContainerVersionContentInternal(string projectId, ContainerType containerType, string containerId, string containerVersion, [FromForm] InternalDocumentRequest request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContent = await _contentService.PostContainerContentInternal(username, projectId, containerType, containerId, containerVersion, request);

            if (newContent != null)
                return CreatedAtAction("GetContainerVersionContent", new { projectId, containerType, containerId, containerVersion, contentId = newContent.Guid }, newContent.ToJsonLD());
            else
                return BadRequest("Could not find container or create document");
        }

        /// <summary>
        /// Posts new external content to newest version of the container
        /// </summary>
        /// <response code="200">Posts and returns new content as json-ld</response>
        /// <response code="400">Could not create content</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/contents/external")]
        public async Task<IActionResult> PostContainerContentExternal(string projectId, ContainerType containerType, string containerId, [FromForm] ExternalDocumentRequest request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContent = await _contentService.PostContainerContentExternal(username, projectId, containerType, containerId, null, request);

            if (newContent != null)
                return CreatedAtAction("GetContainerContent", new { projectId, containerType, containerId, contentId = newContent.Guid }, newContent.ToJsonLD());
            else
                return BadRequest("Could not find container or create document");
        }

        /// <summary>
        /// Posts new external content to a container
        /// </summary>
        /// <response code="200">Posts and returns new content as json-ld</response>
        /// <response code="400">Could not create content</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/contents/external")]
        public async Task<IActionResult> PostContainerVersionContentExternal(string projectId, ContainerType containerType, string containerId, string containerVersion, [FromForm] ExternalDocumentRequest request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContent = await _contentService.PostContainerContentExternal(username, projectId, containerType, containerId, containerVersion, request);

            if (newContent != null)
                return CreatedAtAction("GetContainerContent", new { projectId, containerType, containerId, contentId = newContent.Guid }, newContent.ToJsonLD());
            else
                return BadRequest("Could not find container or create document");
        }

        /// <summary>
        /// Posts new encrypted content to newest version of the container
        /// </summary>
        /// <response code="200">Posts and returns new content as json-ld</response>
        /// <response code="400">Could not create content</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/contents/encrypted")]
        public async Task<IActionResult> PostContainerContentEncrypted(string projectId, ContainerType containerType, string containerId, [FromForm] EncryptedDocumentRequest request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContent = await _contentService.PostContainerContentEncrypted(username, projectId, containerType, containerId, null, request);

            if (newContent != null)
                return CreatedAtAction("GetContainerContent", new { projectId, containerType, containerId, contentId = newContent.Guid }, newContent.ToJsonLD());
            else
                return BadRequest("Could not find container or create document");
        }

        /// <summary>
        /// Posts new encrypted content to a container
        /// </summary>
        /// <response code="200">Posts and returns new content as json-ld</response>
        /// <response code="400">Could not create content</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/contents/encrypted")]
        public async Task<IActionResult> PostContainerVersionContentEncrypted(string projectId, ContainerType containerType, string containerId, string containerVersion, [FromForm] EncryptedDocumentRequest request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContent = await _contentService.PostContainerContentEncrypted(username, projectId, containerType, containerId, containerVersion, request);

            if (newContent != null)
                return CreatedAtAction("GetContainerContent", new { projectId, containerType, containerId, contentId = newContent.Guid }, newContent.ToJsonLD());
            else
                return BadRequest("Could not find container or create document");
        }

        /// <summary>
        /// Posts new folder content to newest version of the container
        /// </summary>
        /// <response code="200">Posts and returns new content as json-ld</response>
        /// <response code="400">Could not create content</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/contents/folder")]
        public async Task<IActionResult> PostContainerContentFolder(string projectId, ContainerType containerType, string containerId, [FromForm] FolderDocumentRequest request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContent = await _contentService.PostContainerContentFolder(username, projectId, containerType, containerId, null, request);

            if (newContent != null)
                return CreatedAtAction("GetContainerContent", new { projectId, containerType, containerId, contentId = newContent.Guid }, newContent.ToJsonLD());
            else
                return BadRequest("Could not find container or create document");
        }

        /// <summary>
        /// Posts new folder content to a container
        /// </summary>
        /// <response code="200">Posts and returns new content as json-ld</response>
        /// <response code="400">Could not create content</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/contents/folder")]
        public async Task<IActionResult> PostContainerVersionContentFolder(string projectId, ContainerType containerType, string containerId, string containerVersion, [FromForm] FolderDocumentRequest request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContent = await _contentService.PostContainerContentFolder(username, projectId, containerType, containerId, containerVersion, request);

            if (newContent != null)
                return CreatedAtAction("GetContainerContent", new { projectId, containerType, containerId, contentId = newContent.Guid }, newContent.ToJsonLD());
            else
                return BadRequest("Could not find container or create document");
        }

        /// <summary>
        /// Posts new secured content to newest version of the container
        /// </summary>
        /// <response code="200">Posts and returns new content as json-ld</response>
        /// <response code="400">Could not create content</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/contents/secured")]
        public async Task<IActionResult> PostContainerContentSecured(string projectId, ContainerType containerType, string containerId, [FromForm] SecuredDocumentRequest request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContent = await _contentService.PostContainerContentSecured(username, projectId, containerType, containerId, null, request);

            if (newContent != null)
                return CreatedAtAction("GetContainerContent", new { projectId, containerType, containerId, contentId = newContent.Guid }, newContent.ToJsonLD());
            else
                return BadRequest("Could not find container or create document");
        }

        /// <summary>
        /// Posts new secured content to a container
        /// </summary>
        /// <response code="200">Posts and returns new content as json-ld</response>
        /// <response code="400">Could not create content</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/contents/secured")]
        public async Task<IActionResult> PostContainerVersionContentSecured(string projectId, ContainerType containerType, string containerId, string containerVersion, [FromForm] SecuredDocumentRequest request)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newContent = await _contentService.PostContainerContentSecured(username, projectId, containerType, containerId, containerVersion, request);

            if (newContent != null)
                return CreatedAtAction("GetContainerContent", new { projectId, containerType, containerId, contentId = newContent.Guid }, newContent.ToJsonLD());
            else
                return BadRequest("Could not find container or create document");
        }


        /// <summary>
        /// Gets a specified content inside container
        /// </summary>
        /// <response code="200">Returns content as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/contents/{contentId}")]
        public async Task<IActionResult> GetContainerContent(string projectId, ContainerType containerType, string containerId, string contentId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var content = await _contentService.GetContainerContent(projectId, containerType, containerId, null, contentId);

            if (content != null)
                return Ok(content.ToJsonLD());
            else
                return NotFound("Could not find container or content inside container");
        }

        /// <summary>
        /// Gets a specified content inside container version
        /// </summary>
        /// <response code="200">Returns content as json-ld</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/contents/{contentId}")]
        public async Task<IActionResult> GetContainerVersionContent(string projectId, ContainerType containerType, string containerId, string containerVersion, string contentId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var content = await _contentService.GetContainerContent(projectId, containerType, containerId, containerVersion, contentId);

            if (content != null)
                return Ok(content.ToJsonLD());
            else
                return NotFound("Could not find container or content inside container");
        }

        /// <summary>
        /// Gets newest version of the container as a file
        /// </summary>
        /// <response code="200">Returns content as file</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/contents/{contentId}/attachment")]
        public async Task<IActionResult> GetContainerContentAsFile(string projectId, ContainerType containerType, string containerId, string contentId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var content = await _contentService.GetContainerContentAsFile(projectId, containerType, containerId, null, contentId);

            if (content != null)
                return File(content.Content, content.ContentType, content.FileName);
            else
                return NotFound("Could not find container or content inside container");
        }

        /// <summary>
        /// Gets the container as a file
        /// </summary>
        /// <response code="200">Returns content as file</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/contents/{contentId}/attachment")]
        public async Task<IActionResult> GetContainerVersionContentAsFile(string projectId, ContainerType containerType, string containerId, string containerversion, string contentId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var content = await _contentService.GetContainerContentAsFile(projectId, containerType, containerId, containerversion, contentId);

            if (content != null)
                return File(content.Content, content.ContentType, content.FileName);
            else
                return NotFound("Could not find container or content inside container");
        }

        /// <summary>
        /// Deletes content in container
        /// </summary>
        /// <response code="200">Deletes content</response>
        /// <response code="400">Could not delete content</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="404">The Project or container does not exist</response>
        /// <response code="500">Internal Server Error</response>
        [HttpDelete("projects/{projectId}/containerTypes/{containerType}/containers/{containerId}/versions/{containerVersion}/contents")]
        public async Task<IActionResult> DeleteContainerVersionContent(string projectId, ContainerType containerType, string containerId, string containerVersion, string contentId)
        {
            if (!ProjectExists(projectId))
                return NotFound("The Project does not exist");

            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var result = await _contentService.DeleteContent(username, projectId, containerType, containerId, containerVersion, contentId);

            if (result)
                return Ok("Content " + contentId + " has been deleted");
            else
                return BadRequest("Could not delete content");
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
