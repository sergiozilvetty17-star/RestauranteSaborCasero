using Microsoft.AspNetCore.Mvc;

namespace nombtre.Controllers
{
    public class InventarioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
