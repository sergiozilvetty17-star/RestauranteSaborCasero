using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Data;
using RestauranteSaborCasero.Models;

namespace RestauranteSaborCasero.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ComprasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComprasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET: Compras
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var compras = await _context.Compras
                .Include(c => c.Usuario)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Ingrediente)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            return View(compras);
        }


        // ==========================================
        // GET: Compras/Details/5
        // ==========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var compra = await _context.Compras
                .Include(c => c.Usuario)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Ingrediente)
                .FirstOrDefaultAsync(c => c.IdCompra == id);

            if (compra == null)
                return NotFound();

            return View(compra);
        }


        // ==========================================
        // GET: Compras/Create
        // ==========================================

        public async Task<IActionResult> Create()
        {
            await CargarUsuarios();

            return View();
        }


        // ==========================================
        // POST: Compras/Create
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Compra compra)
        {
            // Ignorar las propiedades de navegación en la validación
            ModelState.Remove("Usuario");
            ModelState.Remove("Detalles");

            if (ModelState.IsValid)
            {
                if (compra.Fecha == default)
                {
                    compra.Fecha = DateTime.Now.Date;
                }

                _context.Compras.Add(compra);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await CargarUsuarios(compra.IdUsuario);

            return View(compra);
        }

        // ==========================================
        // GET: Compras/Edit/5
        // ==========================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var compra = await _context.Compras
                .FindAsync(id);

            if (compra == null)
                return NotFound();

            await CargarUsuarios(compra.IdUsuario);

            return View(compra);
        }


        // ==========================================
        // POST: Compras/Edit/5
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Compra compra)
        {
            if (id != compra.IdCompra)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(compra);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Compras
                        .AnyAsync(c => c.IdCompra == id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await CargarUsuarios(compra.IdUsuario);

            return View(compra);
        }


        // ==========================================
        // CARGAR USUARIOS
        // ==========================================

        private async Task CargarUsuarios(int? usuarioSeleccionado = null)
        {
            var usuarios = await _context.Usuarios
                .Where(u => u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            ViewData["IdUsuario"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                usuarios,
                "IdUsuario",
                "Nombre",
                usuarioSeleccionado
            );
        }
    }
}