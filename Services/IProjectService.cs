using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.Authentication;

namespace IcddWebApp.Services
{
    public interface IProjectService
    {
        Task<List<Project>> GetProjects();
        Task<List<Project>> GetUserProjects(string username);
        Task<Project> GetProject(string projectId, string username);
        Task<Project> PostProject(Project project, string username);
        Task<bool> UpdateProject(Project updateProject, string username);
        //Task<List<User>> GetProjectUsers(string projectId);
        Task<bool> DeleteProject(string projectId, string username);
        Task<bool> UserBelongsToProject(string username, string projectId);
        Task<User> AddUserToProject(string username, string projectId);
        Task<bool> RemoveUserFromProject(string username, string projectId);
    }
}
