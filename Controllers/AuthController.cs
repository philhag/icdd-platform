using Microsoft.AspNetCore.Mvc;

namespace IcddWebApp.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
