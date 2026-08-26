using Microsoft.AspNetCore.Mvc;

namespace SaborCaseroRestaurante.Controllers
{
    public class ComprasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
