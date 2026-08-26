using Microsoft.AspNetCore.Mvc;

namespace SaborCaseroRestaurante.Controllers
{
    public class MesasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
