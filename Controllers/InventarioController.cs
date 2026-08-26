using Microsoft.AspNetCore.Mvc;

namespace SaborCaseroRestaurante.Controllers
{
    public class InventarioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
