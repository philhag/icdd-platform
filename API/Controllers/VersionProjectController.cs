using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Data;
using IcddWebApp.Services;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.DTOs;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IcddWebApp.API.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("api/v{apiVersion:apiVersion}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VersionProject : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IProjectService _projectService;

        public VersionProject(DatabaseContext context, IProjectService projectService)
        {
            _context = context;
            _projectService = projectService;
        }

        /// <summary>
        /// Returns existing API versions
        /// </summary>
        /// <response code="200">Returns versions</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="500">Internal Server Error</response>
        [ApiVersionNeutral]
        [AllowAnonymous]
        [HttpGet("versions")]
        public async Task<ActionResult<IEnumerable<VersionApi>>> GetVersions()
        {
            return await _context.Versions.ToListAsync();
        }

        /// <summary>
        /// Returns existing projects
        /// </summary>
        /// <response code="200">Returns projects</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="500">Internal Server Error</response>
        [EnableQuery]
        [HttpGet("projects")]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetUserProjects()
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var projects = await _projectService.GetUserProjects(username);
            var projectDTOs = new List<ProjectDTO>();
            foreach(var proj in projects)
            {
                projectDTOs.Add(new ProjectDTO(proj));
            }
            return Ok(projectDTOs);
        }

        /// <summary>
        /// Returns project by ID
        /// </summary>
        /// <response code="200">Returns project</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="400">The Project does not exist or no permission to view project details</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("projects/{projectId}")]
        public async Task<ActionResult<ProjectDTO>> GetProject(string projectId)
        {
            var project = await _projectService.GetProject(projectId, User.Claims.FirstOrDefault(c => c.Type == "Username").Value);

            if (project != null)
                return Ok(new ProjectDTO(project));
            else
                return BadRequest("Project does not exist or no permission to view project details");
        }
    }
}
