using IcddWebApp.Services.Models.Authentication;

namespace IcddWebApp.Services.Models.DTOs
{
    public class UserDTO
    {
        public string Username { get; set; }
        public string? Email { get; set; }

        public UserDTO() { }

        public UserDTO(User user)
        {
            Username = user.UserName;
            Email = user.Email;
        }
    }
}
