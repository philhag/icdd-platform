using Microsoft.AspNetCore.Mvc;

namespace IcddWebApp.Controllers
{
    public class PageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Documentation()
        {
            return View();
        }
    }
}
