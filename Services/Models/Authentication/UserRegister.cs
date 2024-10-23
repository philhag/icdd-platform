namespace IcddWebApp.Services.Models.Authentication
{
    public class UserRegister
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public UserRegister() { }

        public UserRegister(string username, string email, string password)
        {
            Username = username;
            Email = email;
            Password = password;
        }
    }
}
