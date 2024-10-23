using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using IcddWebApp.Data;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.Authentication;
using IcddWebApp.WebApplication.Environment;
using Microsoft.AspNetCore.Identity;

namespace IcddWebApp.Services
{
    public class ProjectService : IProjectService
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _workfolderPath;
        private readonly IContainerService _containerService;
        private readonly IAuthService _authService;
        private UserManager<User> _userManager;

        public ProjectService(DatabaseContext context, IConfiguration configuration, IContainerService containerService, IAuthService authService, UserManager<User> userManager)
        {
            _context = context;
            _configuration = configuration;
            _workfolderPath = _configuration["WorkfolderPath"];
            _containerService = containerService;
            _authService = authService;
            _userManager = userManager;
        }

        public async Task<List<Project>> GetProjects()
        {
            var projects = await _context.Projects.Include(m => m.Containers).Include(m => m.Users).ToListAsync();
            return projects;
        }

        public async Task<List<Project>> GetUserProjects(string username)
        {
            var user = await _authService.GetUser(username, username);
            return user.Projects;
        }

        public async Task<Project> GetProject(string projectId, string username)
        {
            var user = await _authService.GetUser(username, username);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            Project project = null;

            if (isAdmin)
                project = await _context.Projects.Where(x => x.Id == projectId).Include(m => m.Containers).Include(m => m.Users).SingleOrDefaultAsync();
            else
                project = await _context.Projects.Where(x => x.Id == projectId && x.Users.Contains(user)).Include(m => m.Containers).Include(m => m.Users).SingleOrDefaultAsync();

            if (project != null)
                return project;
            else
                return null;

        }

        public async Task<User> AddUserToProject(string username, string projectId)
        {
            var user = await _authService.GetUser(username, username);
            var project = await _context.Projects.Where(x => x.Id == projectId).SingleOrDefaultAsync();

            if (user == null || project == null)
            {
                return null;
            }
            else
            {
                user.Projects.Add(project);
                project.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
        }

        public async Task<Project> PostProject(Project project, string username)
        {
            if (_context.Projects.Any(e => e.Id == project.Id))
            {
                return null;
            }
            else
            {
                if (project.Id == null)
                    project.Id = GuidEncoder.Encode(Guid.NewGuid());

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                await AddUserToProject(username, project.Id);

                try
                {
                    var projectPath = Path.Combine(_workfolderPath, project.Id);
                    if (!Directory.Exists(projectPath))
                        Directory.CreateDirectory(projectPath);

                }
                catch (Exception)
                {
                    throw new Exception("Could not create project folder");
                }
                return await GetProject(project.Id, username);
            }
        }

        public async Task<bool> UpdateProject(Project updateProject, string username)
        {
            var project = await GetProject(updateProject.Id, username);

            project.Modified = DateTime.Now;
            project.Name = updateProject.Name;

            _context.Projects.Update(project);
            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                var projectExists = await _context.Projects.FindAsync(project);
                if (projectExists != null)
                {
                    return false;
                }
                else
                {
                    throw new Exception("Could not update project");
                }
            }
            return true;
        }

        //public async Task<List<User>> GetProjectUsers (string projectId)
        //{
        //    var project = await GetProject(projectId);
        //    return await _context.ContextUsers.Where(x => x.Projects.Contains(project)).ToListAsync();
        //}

        public async Task<bool> UserBelongsToProject(string username, string projectId)
        {
            var user = await _context.ContextUsers.Where(x => x.UserName == username).Include("Projects").SingleOrDefaultAsync();
            var project = await _context.Projects.FindAsync(projectId);

            return user.Projects.Contains(project);
        }

        public async Task<bool> RemoveUserFromProject(string username, string projectId)
        {
            var project = await GetProject(projectId, username);
            var user = await _context.ContextUsers.Where(x => x.UserName == username).SingleOrDefaultAsync();

            if (user != null && project != null)
            {
                project.Users.Remove(user);
                user.Projects.Remove(project);
                await _context.SaveChangesAsync();
                return true;
            }
            else
                return false;
        }

        public async Task<bool> DeleteProject(string projectId, string username)
        {
            var project = await GetProject(projectId, username);

            if (project == null)
                return false;

            try
            {
                var projectPath = Path.Combine(_workfolderPath, projectId);
                if (Directory.Exists(projectPath))
                    Directory.Delete(projectPath, true);
            }
            catch (Exception)
            {
                throw new Exception("Could not delete project file or one of its container files");
            }

            foreach (var container in project.Containers.ToList())
            {
                await _containerService.DeleteContainer(username, container.ProjectId, container.Type, container.Id, container.Version);
            }
            _context.Entry(project).State = EntityState.Deleted;
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
