using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IcddWebApp.Data;
using IcddWebApp.Services.Models.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace IcddWebApp.Services
{
    public class AuthService : IAuthService
    {
        private DatabaseContext _context;
        private readonly IConfiguration Configuration;
        private UserManager<User> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public AuthService(DatabaseContext context, IConfiguration config, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            Configuration = config;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<UserResponse> RegisterUser(UserRegister user)
        {
            if (!UserExists(user))
            {
                var newUser = await _userManager.CreateAsync(new User(user.Username, user.Email), user.Password);

                if (newUser.Succeeded)
                {
                    var userDb = await _userManager.FindByNameAsync(user.Username);
                    await _userManager.AddToRoleAsync(userDb, "User");
                    await _context.SaveChangesAsync();

                    return new UserResponse(userDb);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> CreateUserRole(string role)
        {
            var newRole = new IdentityRole(role);
            var result = await _roleManager.CreateAsync(newRole);
            var newClaim = new Claim("Role", role);
            await _roleManager.AddClaimAsync(newRole, newClaim);
            return result.Succeeded;
        }

        public async Task<User> GetUser(string username, string authUsername)
        {
            var activeUser = await _context.ContextUsers.Where(x => x.UserName == authUsername).SingleOrDefaultAsync();
            bool isAdmin = await _userManager.IsInRoleAsync(activeUser, "Admin");
            if (username == authUsername)
            {
                var user = await _context.ContextUsers.Where(x => x.UserName == authUsername).Include(x => x.Projects).ThenInclude(y => y.Containers).SingleOrDefaultAsync();
                return user != null ? user : null;
            }
            else if (isAdmin)
            {
                var user = await _context.ContextUsers.Where(x => x.UserName == username).Include(x => x.Projects).ThenInclude(y => y.Containers).SingleOrDefaultAsync();
                return user != null ? user : null;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<User>> GetUsers()
        {
            return await _context.ContextUsers.Include(x => x.Projects).ToListAsync();
        }

        public async Task<ObjectResult> UserLogin(UserLogin user)
        {
            // var verifyUser = Authenticate(user.Password, user.Username);
            var userDb = await _userManager.FindByNameAsync(user.Username);

            if (user != null && user.Username != null && user.Password != null && await _userManager.CheckPasswordAsync(userDb, user.Password))
            {
                var token = await CreateToken(userDb);
                var refresh = CreateRefreshToken();

                userDb.RefreshToken = refresh;
                userDb.RefreshTokenExpiration = DateTime.Now.AddDays(1);

                await _context.SaveChangesAsync();

                //Response.Headers.Add("access-token-expiration", token.ValidTo.ToString());
                //Response.Headers.Add("refresh-token-expiration", verifyUser.RefreshTokenExpiration.ToString());
                return new ObjectResult(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    refreshToken = refresh
                });
            }
            else
            {
                return null;
            }
        }

        //public User Authenticate(string password, string username) //string projectId
        //{
        //    var user = _context.Users.SingleOrDefault(x => x.UserName == username);
        //    if (user == null)
        //        throw new Exception("The user does not exist.");


        //    if (Validate(password, user.PasswordHash, user.PasswordSalt)) //&& user.ProjectIds.Contains(projectId)
        //        return user;
        //    else
        //        throw new Exception("Could not authenticate user.");

        //}

        //public string CreateHash(string plain, string salt)
        //{
        //    var valueBytes = KeyDerivation.Pbkdf2(
        //                 password: plain,
        //                 salt: Encoding.UTF8.GetBytes(salt),
        //                 prf: KeyDerivationPrf.HMACSHA512,
        //                 iterationCount: 10000,
        //                 numBytesRequested: 256 / 8);

        //    return Convert.ToBase64String(valueBytes);
        //}

        //public string CreateSalt()
        //{
        //    byte[] randomBytes = new byte[128 / 8];
        //    using (var generator = RandomNumberGenerator.Create())
        //    {
        //        generator.GetBytes(randomBytes);
        //        return Convert.ToBase64String(randomBytes);
        //    }
        //}

        //public bool Validate(string plain, string hash, string salt)
        //{
        //    return CreateHash(plain, salt) == hash;
        //}

        public async Task<JwtSecurityToken> CreateToken(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var isAdmin = userRoles.Contains("Admin");

            var newClaims = new List<Claim>() {
                new Claim(JwtRegisteredClaimNames.Sub, Configuration["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
                new Claim("Id", user.Id.ToString()),
                new Claim("Username", user.UserName),
                new Claim("IsAdmin", isAdmin.ToString())};


            //var claims = new[] {
            //    new Claim(JwtRegisteredClaimNames.Sub, Configuration["Jwt:Subject"]),
            //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
            //    new Claim("Id", user.Id.ToString()),
            //    new Claim("Username", user.UserName),
            //    new Claim("IsAdmin", isAdmin.ToString())
            //};

            foreach (var role in userRoles)
            {
                newClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claims = newClaims.ToArray();


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                Configuration["Jwt:Issuer"],
                Configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: signIn
                );

            return token;
        }

        public string CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetExpiredTokenPrincipal(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public async Task<User> AddUserToRole(string username, string role)
        {
            var user = await _userManager.FindByNameAsync(username);
            var result = await _userManager.AddToRoleAsync(user, role);

            return result.Succeeded ? user : null;
        }

        public async Task<User> RemoveUserFromRole(string username, string role)
        {
            var user = await _userManager.FindByNameAsync(username);
            var result = await _userManager.RemoveFromRoleAsync(user, role);

            return result.Succeeded ? user : null;
        }

        public bool IsExpired(DateTime expirationDate)
        {
            return expirationDate < DateTime.Now;
        }

        public async Task<User> SetUserDescription(string username, string description)
        {
            var user = await _userManager.FindByNameAsync(username);
            user.Description = description;
            try
            {
                _context.Users.Update(user);
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return null;
            }
            return user;
        }

        public async Task<User> SetUserOrganisation(string username, string organisation)
        {
            var user = await _userManager.FindByNameAsync(username);
            user.Organisation = organisation;
            try
            {
                _context.Users.Update(user);
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return null;
            }
            return user;
        }
        #region Helpers

        public bool UserExists(UserRegister user)
        {
            return _context.Users.Any(e => e.UserName == user.Username);
        }

        #endregion
    }
}
