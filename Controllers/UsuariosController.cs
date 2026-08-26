using Microsoft.AspNetCore.Mvc;

namespace SaborCaseroRestaurante.Controllers
{
    public class CocinerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
