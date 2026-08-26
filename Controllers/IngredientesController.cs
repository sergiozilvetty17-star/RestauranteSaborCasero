using Microsoft.AspNetCore.Mvc;

namespace SaborCaseroRestaurante.Controllers
{
    public class IngredientesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
