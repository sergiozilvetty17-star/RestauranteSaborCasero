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
            await CargarIngredientes();
            return View();
        }

        // ==========================================
        // POST: Compras/Create
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Compra compra)
        {
            ModelState.Remove("Usuario");
            if (compra.Detalles != null)
            {
                for (int i = 0; i < compra.Detalles.Count; i++)
                {
                    ModelState.Remove($"Detalles[{i}].Compra");
                    ModelState.Remove($"Detalles[{i}].Ingrediente");
                }
            }
            else
            {
                ModelState.AddModelError("Detalles", "Debes agregar al menos un ingrediente.");
            }

            if (ModelState.IsValid)
            {
                if (compra.Fecha == default)
                {
                    compra.Fecha = DateTime.Now.Date;
                }

                _context.Compras.Add(compra);
                await _context.SaveChangesAsync();

                // Sumar al inventario
                foreach (var detalle in compra.Detalles)
                {
                    var ingrediente = await _context.Ingredientes.FindAsync(detalle.IdIngrediente);
                    if (ingrediente != null)
                    {
                        ingrediente.CantidadDisponible += detalle.Cantidad;
                        _context.Update(ingrediente);
                    }
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await CargarUsuarios(compra.IdUsuario);
            await CargarIngredientes();
            return View(compra);
        }

        // ==========================================
        // GET: Compras/Edit/5
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var compra = await _context.Compras
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.IdCompra == id);

            if (compra == null) return NotFound();

            await CargarUsuarios(compra.IdUsuario);
            await CargarIngredientes();
            return View(compra);
        }

        // ==========================================
        // POST: Compras/Edit/5
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Compra compra)
        {
            if (id != compra.IdCompra) return NotFound();

            ModelState.Remove("Usuario");
            if (compra.Detalles != null)
            {
                for (int i = 0; i < compra.Detalles.Count; i++)
                {
                    ModelState.Remove($"Detalles[{i}].Compra");
                    ModelState.Remove($"Detalles[{i}].Ingrediente");
                }
            }
            else
            {
                ModelState.AddModelError("Detalles", "Debes agregar al menos un ingrediente.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var compraOriginal = await _context.Compras
                        .Include(c => c.Detalles)
                        .FirstOrDefaultAsync(c => c.IdCompra == id);

                    if (compraOriginal == null) return NotFound();

                    // Revertir stock anterior
                    foreach (var detalle in compraOriginal.Detalles)
                    {
                        var ing = await _context.Ingredientes.FindAsync(detalle.IdIngrediente);
                        if (ing != null) ing.CantidadDisponible -= detalle.Cantidad;
                    }

                    _context.DetalleCompras.RemoveRange(compraOriginal.Detalles);

                    compraOriginal.IdUsuario = compra.IdUsuario;
                    compraOriginal.Fecha = compra.Fecha;
                    compraOriginal.Proveedor = compra.Proveedor;

                    compraOriginal.Detalles = new List<DetalleCompra>();
                    foreach (var detalle in compra.Detalles)
                    {
                        compraOriginal.Detalles.Add(new DetalleCompra
                        {
                            IdIngrediente = detalle.IdIngrediente,
                            Cantidad = detalle.Cantidad
                        });

                        var ing = await _context.Ingredientes.FindAsync(detalle.IdIngrediente);
                        if (ing != null) ing.CantidadDisponible += detalle.Cantidad;
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Compras.AnyAsync(c => c.IdCompra == id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await CargarUsuarios(compra.IdUsuario);
            await CargarIngredientes();
            return View(compra);
        }

        // ==========================================
        // CARGAR INGREDIENTES
        // ==========================================
        private async Task CargarIngredientes()
        {
            ViewData["Ingredientes"] = await _context.Ingredientes
                .OrderBy(i => i.Nombre)
                .ToListAsync();
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