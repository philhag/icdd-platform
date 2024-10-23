using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.Data;
using IcddWebApp.Services;
using IcddWebApp.Services.Models.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IcddWebApp.API.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;
        private readonly IProjectService _projectService;

        public AuthController(IAuthService authService, DatabaseContext context, UserManager<User> userManager, IProjectService projectService)
        {
            _authService = authService;
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
        }

        /// <summary>
        /// Logs in user to authorize further requests
        /// </summary>
        /// <response code="200">Returns access and refresh token including their expiration time (header)</response>
        /// <response code="400">Invalid user credentials</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="403">No permission for the requested operation</response>
        /// <response code="500">Internal Server Error</response>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> UserLogin(UserLogin user)
        {   
             if (user != null && user.Username != null && user.Password != null)
            {
                var userDb = await _userManager.FindByNameAsync(user.Username);
                var userVerified = await _userManager.CheckPasswordAsync(userDb, user.Password);

                if(userVerified)
                {
                    var token = await _authService.CreateToken(userDb);
                    var refresh = _authService.CreateRefreshToken();

                    userDb.RefreshToken = refresh;
                    userDb.RefreshTokenExpiration = DateTime.Now.AddDays(1);

                    await _context.SaveChangesAsync();

                    Response.Headers.Add("access-token-expiration", token.ValidTo.ToString());
                    Response.Headers.Add("refresh-token-expiration", userDb.RefreshTokenExpiration.ToString());
                    return new ObjectResult(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        refreshToken = refresh
                    });
                }
                else
                {
                    return BadRequest("Username or Password is incorrect");
                }
            }
            else
            {
                return BadRequest("Invalid credentials");
            }
        }

        /// <summary>
        /// Invalidates refresh token
        /// </summary>
        /// <response code="200">No Content</response>
        /// <response code="400">Token is invalid</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="403">No permission for the requested operation</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("logout")]
        public async Task<ActionResult> UserLogout()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "Id").Value;
            var user = await _context.ContextUsers.Where(x => x.Id == userId).SingleOrDefaultAsync();

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiration = DateTime.Now;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            else
            {
                return BadRequest("Token is invalid");
            }
        }

        /// <summary>
        /// Returns refresh token
        /// </summary>
        /// <response code="200">Returns new access and refresh token including their expiration time (header)</response>
        /// <response code="400">Refresh token expired or does not belong to access token</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="403">No permission for the requested operation</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshToken(string token, string refreshToken)
        {
            var principal = _authService.GetExpiredTokenPrincipal(token);
            var user = principal.Claims.FirstOrDefault(c => c.Type == "Id").Value;

            var requestUser = await _context.ContextUsers.Where(x => x.Id == user).SingleOrDefaultAsync();

            if (requestUser == null || requestUser.RefreshToken != refreshToken || _authService.IsExpired(requestUser.RefreshTokenExpiration)) 
                return BadRequest("Could not refresh Token");

            var newToken = await _authService.CreateToken(requestUser);
            var newRefreshToken = _authService.CreateRefreshToken();

            requestUser.RefreshToken = newRefreshToken;
            requestUser.RefreshTokenExpiration = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();

            Response.Headers.Add("access-token-expiration", newToken.ValidTo.ToString());
            Response.Headers.Add("refresh-token-expiration", requestUser.RefreshTokenExpiration.ToString());

            return new ObjectResult(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(newToken),
                refreshToken = newRefreshToken
            });
        }

        /// <summary>
        /// Returns list of existing users (admins only)
        /// </summary>
        /// <response code="200">Returns list of existing users</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="403">No permission for the requested operation</response>
        /// <response code="500">Internal Server Error</response>
        [Authorize(Roles ="Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
        {
            var users = await _authService.GetUsers();
            var userResponses = new List<UserResponse>();
            foreach (var user in users)
                userResponses.Add(new UserResponse(user, await _userManager.GetRolesAsync(user)));
            return Ok(userResponses);
        }

        /// <summary>
        /// Returns user by username
        /// </summary>
        /// <response code="200">Returns user</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="403">No permission for the requested operation</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("user/{username}")]
        public async Task<ActionResult<UserResponse>> GetUser(string username)
        {
            var authUsername = User.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var user = await _authService.GetUser(username, authUsername);

            var test = User.IsInRole("Admin");

            if (user != null)
                return Ok(new UserResponse(user, await _userManager.GetRolesAsync(user)));
            else
                return BadRequest("Could not find user or no permission");
        }
            
        /// <summary>
        /// Register new user
        /// </summary>
        /// <response code="200">Returns newly created user</response>
        /// <response code="401">User needs to be logged in</response>
        /// <response code="403">No permission for the requested operation</response>
        /// <response code="500">Internal Server Error</response>
        [AllowAnonymous]
        [HttpPost("registeruser")]
        public async Task<ActionResult<UserResponse>> RegisterUser(UserRegister user)
        {
           var newUser = await _authService.RegisterUser(user);

            if (newUser != null)
                return Ok(newUser);
            else
                return Conflict("Username or email already exists or no permission");
        }

        /// <summary>
        /// Add project to user
        /// </summary>
        /// <response code="200">Returns user</response>
        /// <response code="400">User or project was not found</response>
        /// <response code="500">Internal Server Error</response>
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("user/{username}/project/{projectId}")]
        public async Task<ActionResult<UserResponse>> AddUserToProject(string username, string projectId)
        {
            var user = await _projectService.AddUserToProject(username, projectId);

            if (user != null)
                return Ok(new UserResponse(user));
            else
                return NotFound("User or project not found");
        }

        /// <summary>
        /// Add role to user
        /// </summary>
        /// <response code="200">Returns user</response>
        /// <response code="400">User was not found</response>
        /// <response code="500">Internal Server Error</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("user/{username}/role/{role}")]
        public async Task<ActionResult<User>> AddRoleToUser(string username, string role)
        {
            var user = await _authService.AddUserToRole(username, role);

            if (user != null)
                return CreatedAtAction("GetUser", new { username = user.UserName }, user);
            else
                return NotFound("User or project not found");
        }

        /// <summary>
        /// Add role to user
        /// </summary>
        /// <response code="200">Returns user</response>
        /// <response code="400">User was not found</response>
        /// <response code="500">Internal Server Error</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("user/role")]
        public async Task<ActionResult> PostUserRole(string role)
        {
            var result = await _authService.CreateUserRole(role);

            if (result)
                return Ok("Role has been successfully created");
            else
                return BadRequest("Could not create role");
        }
    }
}
