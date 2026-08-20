using Microsoft.AspNetCore.Mvc;

namespace nombtre.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
