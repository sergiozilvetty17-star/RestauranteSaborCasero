using Microsoft.AspNetCore.Mvc;

namespace nombtre.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
