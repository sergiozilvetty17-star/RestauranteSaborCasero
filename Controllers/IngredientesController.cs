using Microsoft.AspNetCore.Mvc;

namespace nombtre.Controllers
{
    public class IngredientesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
