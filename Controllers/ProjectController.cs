using System;
using System.Threading.Tasks;
using IcddWebApp.PageModels.Error;
using IcddWebApp.PageModels.Project;
using IcddWebApp.Services;
using IcddWebApp.Services.Models;
using IcddWebApp.WebApplication.Environment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace IcddWebApp.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IProjectService _projectService;
        private IAuthService _authService;

        public ProjectController(IWebHostEnvironment environment, IProjectService projectService, IAuthService authService)
        {
            _environment = environment;
            _projectService = projectService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var username = User.Identity.Name;
            var activeUser = await _authService.GetUser(username, username);

            if (activeUser != null)
                return View(new ProjectPageModel(activeUser));
            else
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find active user.",
                            new FormatException(), $"~/Project/List"));
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var project = await _projectService.GetProject(id, User.Identity.Name);
            if (project != null && await _projectService.UserBelongsToProject(User.Identity.Name, project.Id))
                return View(new ProjectDetailPageModel(project));
            else
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find project or user does not belong to project.",
                            new FormatException(), $"~/Project/Details/" + id));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var project = await _projectService.GetProject(id, User.Identity.Name);
            if (project != null && await _projectService.UserBelongsToProject(User.Identity.Name, project.Id))
                return View(new ProjectDetailPageModel(project));
            else
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not delete project.",
                             new FormatException(), $"~/Project/Details/" + id));
        }

        [HttpPost]
        public async Task<IActionResult> AddProject(string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
                return RedirectToAction("List");

            var newProject = await _projectService.PostProject(new Project(projectName, null, null), User.Identity.Name);

            if (newProject == null)
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not add project.",
                            new FormatException(), $"~/Project/List"));

            Logger.Log($"Projekt erstellt von {User.Identity.Name}:{projectName}" , Logger.MsgType.Info, "AddProject");

            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProject(string id)
        {
            if (!string.IsNullOrEmpty(id) && await _projectService.UserBelongsToProject(User.Identity.Name, id))
            {
                var projectStatus = await _projectService.DeleteProject(id, User.Identity.Name);
                if (projectStatus)
                {
                    Logger.Log($"Projekt gelöscht von {User.Identity.Name}:{id}", Logger.MsgType.Info, "DeleteProject");
                    return RedirectToAction("List");
                }
                    
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not delete project.",
                            new FormatException(), $"~/Project/Details/" + id));
            }
            else
            {
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProject(Project project, string addUser, string deleteUser)
        {
            if (!string.IsNullOrEmpty(project.Id) && await _projectService.UserBelongsToProject(User.Identity.Name, project.Id))
            {
                var projectStatus = await _projectService.UpdateProject(project, User.Identity.Name);
                if(addUser != null)
                {
                    var addResult = await _projectService.AddUserToProject(addUser, project.Id);
                    if (addResult == null)
                        return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not add user to project.",
                            new FormatException(), $"~/Project/Details/" + project.Id));
                }
                if(deleteUser != null)
                {
                    var deleteResult = await _projectService.RemoveUserFromProject(deleteUser, project.Id);
                    if (deleteResult == false)
                        return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not remove user from project.",
                            new FormatException(), $"~/Project/Details/" + project.Id));
                }

                if (projectStatus)
                    return RedirectToAction("Details", new { id = project.Id });
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not update project.",
                            new FormatException(), $"~/Project/Details/" + project.Id));
            }
            else
            {
                return RedirectToAction("List");
            }
        }

    
    }
}
