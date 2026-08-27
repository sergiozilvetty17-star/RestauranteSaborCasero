using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Data;

namespace RestauranteSaborCasero.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            string correo,
            string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                ModelState.AddModelError(
                    "correo",
                    "El correo es obligatorio."
                );
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                ModelState.AddModelError(
                    "contrasena",
                    "La contraseña es obligatoria."
                );
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Correo == correo);

            if (usuario == null || !usuario.Activo)
            {
                ModelState.AddModelError(
                    "",
                    "El correo o la contraseña son incorrectos."
                );

                return View();
            }

            bool contraseñaCorrecta =
                BCrypt.Net.BCrypt.Verify(
                    contrasena,
                    usuario.ContrasenaHash
                );

            if (!contraseñaCorrecta)
            {
                ModelState.AddModelError(
                    "",
                    "El correo o la contraseña son incorrectos."
                );

                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    usuario.Nombre
                ),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Correo
                ),

                new Claim(
                    ClaimTypes.Role,
                    usuario.Rol.ToString()
                )
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true
                }
            );

            return RedirectToAction(
                "Index",
                "Dashboard"
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(nameof(Index));
        }
    }
}