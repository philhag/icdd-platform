using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IcddWebApp.Services.Models.Authentication;

namespace IcddWebApp.Services
{
    public interface IAuthService
    {
        Task<UserResponse> RegisterUser(UserRegister user);
        Task<User> GetUser(string username, string authUsername);
        Task<List<User>> GetUsers();
        Task<bool> CreateUserRole(string role);
       // User Authenticate(string password, string username);
        //string CreateHash(string plain, string salt);
        //string CreateSalt();
        //bool Validate(string plain, string salt, string hash);
        Task<JwtSecurityToken> CreateToken(User user);
        public string CreateRefreshToken();
        public ClaimsPrincipal GetExpiredTokenPrincipal(string token);
        public bool IsExpired(DateTime expirationDate);
        Task<User> AddUserToRole(string username, string role);
        Task<User> RemoveUserFromRole(string username, string role);
        Task<User> SetUserDescription(string username, string description);
        Task<User> SetUserOrganisation(string username, string organisation);
    }
}
