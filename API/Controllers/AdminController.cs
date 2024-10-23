using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Data;
using IcddWebApp.Services;
using IcddWebApp.Services.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcddWebApp.API.Controllers
{
    [ApiVersionNeutral]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IProjectService _projectService;

        public AdminController(DatabaseContext context, IProjectService projectService)
        {
            _context = context;
            _projectService = projectService;
        }

        [HttpPost("versions")]
        public async Task<ActionResult<VersionApi>> PostVersion(VersionApi version)
        {
            _context.Versions.Add(version);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetVersions", new { controller = "container", projectId = version.Id }, version);
        }

        [HttpPost("projects")]
        public async Task<ActionResult<Project>> PostProject(Project project)
        {
            var authUsername = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var newProject = await _projectService.PostProject(project, authUsername);
            if (newProject != null)
                return CreatedAtAction("GetProject", "versionproject", new { projectId = newProject.Id }, newProject);
            else
                return BadRequest("Could not create Project");
        }
    }  
}
