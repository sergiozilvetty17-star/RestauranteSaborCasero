using Microsoft.AspNetCore.Mvc;

namespace nombtre.Controllers
{
    public class CocinerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
