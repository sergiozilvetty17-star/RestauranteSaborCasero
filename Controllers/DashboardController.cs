using Microsoft.AspNetCore.Mvc;

namespace SaborCaseroRestaurante.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
