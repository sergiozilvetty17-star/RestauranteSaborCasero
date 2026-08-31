using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Data;
using RestauranteSaborCasero.Models;

namespace RestauranteSaborCasero.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET: Usuarios
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return View(usuarios);
        }


        // ==========================================
        // GET: Usuarios/Details/5
        // ==========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }


        // ==========================================
        // GET: Usuarios/Create
        // ==========================================

        public IActionResult Create()
        {
            return View();
        }


        // ==========================================
        // POST: Usuarios/Create
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            // ==========================================
            // VERIFICAR CORREO DUPLICADO
            // ==========================================

            bool correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo == usuario.Correo);

            if (correoExiste)
            {
                ModelState.AddModelError(
                    "Correo",
                    "Ya existe un usuario registrado con este correo."
                );
            }


            // ==========================================
            // VALIDAR MODELO
            // ==========================================

            if (ModelState.IsValid)
            {
                // ==========================================
                // ENCRIPTAR CONTRASEÑA CON BCRYPT
                // ==========================================

                usuario.ContrasenaHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        usuario.ContrasenaHash
                    );


                // ==========================================
                // GUARDAR USUARIO
                // ==========================================

                _context.Usuarios.Add(usuario);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }


            return View(usuario);
        }


        // ==========================================
        // GET: Usuarios/Edit/5
        // ==========================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }


        // ==========================================
        // POST: Usuarios/Edit/5
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }


            // ==========================================
            // BUSCAR USUARIO ORIGINAL
            // ==========================================

            var usuarioOriginal = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == id);

            if (usuarioOriginal == null)
            {
                return NotFound();
            }


            // ==========================================
            // VERIFICAR CORREO DUPLICADO
            // ==========================================

            bool correoExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.Correo == usuario.Correo &&
                    u.IdUsuario != usuario.IdUsuario
                );

            if (correoExiste)
            {
                ModelState.AddModelError(
                    "Correo",
                    "Ya existe otro usuario registrado con este correo."
                );
            }


            // ==========================================
            // CONSERVAR CONTRASEÑA ORIGINAL
            // ==========================================

            usuario.ContrasenaHash =
                usuarioOriginal.ContrasenaHash;

            // Quitamos la validación de contraseña porque
            // no estamos cambiándola desde Edit.

            ModelState.Remove(nameof(Usuario.ContrasenaHash));


            // ==========================================
            // VALIDAR MODELO
            // ==========================================

            if (ModelState.IsValid)
            {
                try
                {
                    // ==========================================
                    // ACTUALIZAR DATOS
                    // ==========================================

                    usuarioOriginal.Nombre = usuario.Nombre;
                    usuarioOriginal.Correo = usuario.Correo;
                    usuarioOriginal.Rol = usuario.Rol;
                    usuarioOriginal.Activo = usuario.Activo;


                    // ==========================================
                    // GUARDAR CAMBIOS
                    // ==========================================

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExiste(usuario.IdUsuario))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }


            return View(usuario);
        }


        // ==========================================
        // GET: Usuarios/Delete/5
        // ==========================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }


        // ==========================================
        // POST: Usuarios/Delete/5
        // ==========================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios
                .FindAsync(id);

            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // MÉTODO AUXILIAR
        // ==========================================

        private bool UsuarioExiste(int id)
        {
            return _context.Usuarios
                .Any(e => e.IdUsuario == id);
        }
    }
}
