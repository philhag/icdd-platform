using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.PageModels.Admin;
using IcddWebApp.PageModels.Error;
using IcddWebApp.Services;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IcddWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IProjectService _projectService;
        private readonly IContainerService _containerService;
        private readonly IContentService _contentService;
        private readonly ILinksetService _linksetService;
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly string _workfolderPath;

        public AdminController(IWebHostEnvironment environment, IProjectService projectService, IContainerService containerService, IContentService contentService, ILinksetService linksetService, IAuthService authService, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _environment = environment;
            _projectService = projectService;
            _containerService = containerService;
            _contentService = contentService;
            _linksetService = linksetService;
            _authService = authService;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _workfolderPath = _configuration["WorkfolderPath"];
        }


        public async Task<IActionResult> Index()
        {
            var users = await _authService.GetUsers();
            var model = new AdminIndexPageModel(users);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _authService.GetUsers();
            var userRoles = new Dictionary<string, List<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(user.Id, roles.ToList());
            }
            var model = new ManageUsersPageModel(users, userRoles);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ManageProjects()
        {
            var projects = await _projectService.GetProjects();
            var allUsers = await _authService.GetUsers();
            var model = new ManageProjectsPageModel(projects, allUsers);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ManageRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleUsers = new Dictionary<string, List<User>>();
            var allUsers = await _authService.GetUsers();

            foreach (var role in roles)
            {
                var users = await _userManager.GetUsersInRoleAsync(role.Name);
                roleUsers.Add(role.Id, users.ToList());
            }
            var model = new ManageRolesPageModel(roles, allUsers, roleUsers);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(string roleName)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
                return RedirectToAction("ManageRoles");
            else
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not create role.",
                            new FormatException(), $"~/Admin"));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return RedirectToAction("ManageRoles");
            else
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not delete role.",
                            new FormatException(), $"~/Admin"));
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToRole(string username, string rolename)
        {
            var result = await _authService.AddUserToRole(username, rolename);
            if (result != null)
                return RedirectToAction("ManageRoles");
            else
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not add user to role.",
                            new FormatException(), $"~/Admin"));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromRole(string username, string rolename)
        {
            var result = await _authService.RemoveUserFromRole(username, rolename);
            if (result != null)
                return RedirectToAction("ManageRoles");
            else
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not remove user from role.",
                            new FormatException(), $"~/Admin"));
        }

        [HttpGet]
        public IActionResult Logging()
        {
            var logfilesList = new List<Log>();
            var path = "ProgramLog/";
            if (Directory.Exists(path))
                foreach (var file in Directory.GetFiles(path))
                    logfilesList.Add(new Log(file));
            logfilesList.Sort((u1, u2) => u1.date.CompareTo(u2.date));
            return View(new LoggingPageModel(logfilesList));
        }

        [HttpPost]
        public IActionResult ClearLog(string logfile)
        {
            if (ModelState.IsValid)
            {
                if (!System.IO.File.Exists(logfile)) return RedirectToAction("Logging");
                using (var writer = new StreamWriter(logfile))
                {
                    writer.Write(string.Empty);
                    writer.Close();
                }
            }

            return RedirectToAction("Logging");
        }
        public IActionResult DeleteLog(string logfile)
        {
            if (ModelState.IsValid)
            {
                if (!System.IO.File.Exists(logfile)) return RedirectToAction("Logging");
                System.IO.File.Delete(logfile);
            }
            return RedirectToAction("Logging");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProject(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var projectStatus = await _projectService.DeleteProject(id, User.Identity.Name);
                if (projectStatus)
                    return RedirectToAction("ManageProjects");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not delete project.",
                            new FormatException(), $"~/Admin"));
            }
            else
            {
                return RedirectToAction("ManageProjects");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddProject(string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
                return RedirectToAction("ManageProjects");

            var newProject = await _projectService.PostProject(new Project(projectName, null, null), User.Identity.Name);

            if (newProject == null)
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not add project.",
                            new FormatException(), $"~/Admin"));

            return RedirectToAction("ManageProjects");
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToProject(string projectId, string username)
        {
            if (!string.IsNullOrEmpty(projectId))
            {
                var result = await _projectService.AddUserToProject(username, projectId);

                if (result != null)
                    return RedirectToAction("ManageProjects");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not add user to project.",
                            new FormatException(), $"~/Admin"));
            }
            else
            {
                return RedirectToAction("ManageProjects");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromProject(string projectId, string username)
        {
            if (!string.IsNullOrEmpty(projectId))
            {
                var result = await _projectService.RemoveUserFromProject(username, projectId);

                if (result)
                    return RedirectToAction("ManageProjects");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not remove user from project.",
                            new FormatException(), $"~/Admin"));
            }
            else
            {
                return RedirectToAction("ManageProjects");
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProject(string username, string projectId, string projectName)
        {
            if (!string.IsNullOrEmpty(projectId))
            {
                var project = await _projectService.GetProject(projectId, username);
                project.Name = projectName;
                var result = await _projectService.UpdateProject(project, username);

                if (result)
                    return RedirectToAction("ManageProjects");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not update project.",
                            new FormatException(), $"~/Admin"));
            }
            else
            {
                return RedirectToAction("ManageProjects");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _authService.GetUser(username, username);
                var result = await _userManager.DeleteAsync(user);
                if (result != null)
                    return RedirectToAction("ManageUsers");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not delete user.",
                            new FormatException(), $"~/Admin"));
            }
            else
            {
                return RedirectToAction("ManageUsers");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUnconfirmedUsers(List<string> unconfirmedAccounts)
        {
            foreach (var user in unconfirmedAccounts)
            {
                var account = await _userManager.FindByNameAsync(user);
                if (!account.EmailConfirmed)
                {
                    var result = await _userManager.DeleteAsync(account);
                    if (result == null)
                        return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not delete user " + account.UserName,
                                new FormatException(), $"~/Admin"));
                }
            }
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(string username, string newUsername, string newEmail, bool newEmailConfirmed, string newPhoneNumber, bool newPhoneNumberConfirmed, bool newTwoFactorEnabled)
        {
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _authService.GetUser(username, username);
                user.UserName = newUsername;
                user.Email = newEmail;
                user.EmailConfirmed = newEmailConfirmed;
                user.PhoneNumber = newPhoneNumber;
                user.PhoneNumberConfirmed = newPhoneNumberConfirmed;
                user.TwoFactorEnabled = newTwoFactorEnabled;
                var result = await _userManager.UpdateAsync(user);
                if (result != null)
                    return RedirectToAction("ManageUsers");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not update user.",
                            new FormatException(), $"~/Admin"));
            }
            else
            {
                return RedirectToAction("ManageUsers");
            }
        }

    }
}
