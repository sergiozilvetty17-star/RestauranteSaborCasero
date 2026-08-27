using Microsoft.AspNetCore.Mvc;
using RestauranteSaborCasero.Models;
using System.Diagnostics;

namespace RestauranteSaborCasero.Controllers
{
    public class HomeController : Controller
    {
        // ==========================================
        // GET: Home
        // ==========================================

        public IActionResult Index()
        {
            return View();
        }


        // ==========================================
        // GET: Home/Error
        // ==========================================

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true
        )]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id
                                 ?? HttpContext.TraceIdentifier
                }
            );
        }
    }
}