using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Data;
using RestauranteSaborCasero.Models;

namespace RestauranteSaborCasero.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET: Login
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            // Si ya existe una sesión iniciada,
            // enviamos al usuario al Dashboard.
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );
            }

            return View();
        }


        // ==========================================
        // POST: Login
        // ==========================================

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

            // Buscar usuario por correo.
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Correo == correo);

            // Verificar que exista y esté activo.
            if (usuario == null || !usuario.Activo)
            {
                ModelState.AddModelError(
                    "",
                    "El correo o la contraseña son incorrectos."
                );

                return View();
            }

            // Verificar contraseña utilizando BCrypt.
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

            // ==========================================
            // CREAR CLAIMS
            // ==========================================

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

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            };

            // ==========================================
            // INICIAR SESIÓN
            // ==========================================

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            return RedirectToAction(
                "Index",
                "Dashboard"
            );
        }


        // ==========================================
        // LOGOUT
        // ==========================================

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