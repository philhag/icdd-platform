namespace IcddWebApp.Services.Models.Authentication
{
    public class UserLogin
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public UserLogin() { }

        public UserLogin(string username, string password)
        {
            Username = username;
            Password = password;
        }

    }
}
