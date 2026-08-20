using Microsoft.AspNetCore.Mvc;

namespace nombtre.Controllers
{
    public class MesasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
