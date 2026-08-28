using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Data;
using RestauranteSaborCasero.Models;

namespace RestauranteSaborCasero.Controllers
{
    [Authorize(Roles = "Administrador,Cocinero")]
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET: Inventario
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Ingrediente)
                .OrderByDescending(i => i.Fecha)
                .ToListAsync();

            return View(inventario);
        }


        // ==========================================
        // GET: Inventario/Details/5
        // ==========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var movimiento = await _context.Inventarios
                .Include(i => i.Ingrediente)
                .FirstOrDefaultAsync(i => i.IdInventario == id);

            if (movimiento == null)
                return NotFound();

            return View(movimiento);
        }


        // ==========================================
        // GET: Inventario/Create
        // ==========================================

        public async Task<IActionResult> Create()
        {
            await CargarIngredientes();

            return View();
        }


        // ==========================================
        // POST: Inventario/Create
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inventario inventario)
        {
            if (ModelState.IsValid)
            {
                // Buscar el ingrediente relacionado.
                var ingrediente = await _context.Ingredientes
                    .FirstOrDefaultAsync(i =>
                        i.IdIngrediente == inventario.IdIngrediente);

                if (ingrediente == null)
                {
                    ModelState.AddModelError(
                        "IdIngrediente",
                        "El ingrediente seleccionado no existe."
                    );

                    await CargarIngredientes(
                        inventario.IdIngrediente
                    );

                    return View(inventario);
                }

                // ==========================================
                // APLICAR MOVIMIENTO
                // ==========================================

                switch (inventario.TipoMovimiento)
                {
                    case TipoMovimientoInventario.Entrada:

                        ingrediente.CantidadDisponible +=
                            inventario.Cantidad;

                        break;


                    case TipoMovimientoInventario.Salida:

                        if (ingrediente.CantidadDisponible
                            < inventario.Cantidad)
                        {
                            ModelState.AddModelError(
                                "Cantidad",
                                "No hay suficiente cantidad disponible."
                            );

                            await CargarIngredientes(
                                inventario.IdIngrediente
                            );

                            return View(inventario);
                        }

                        ingrediente.CantidadDisponible -=
                            inventario.Cantidad;

                        break;


                    case TipoMovimientoInventario.Ajuste:

                        ingrediente.CantidadDisponible =
                            inventario.Cantidad;

                        break;
                }

                // Registrar fecha automáticamente.
                inventario.Fecha = DateTime.Now;

                _context.Inventarios.Add(inventario);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await CargarIngredientes(
                inventario.IdIngrediente
            );

            return View(inventario);
        }


        // ==========================================
        // CARGAR INGREDIENTES
        // ==========================================

        private async Task CargarIngredientes(
            int? ingredienteSeleccionado = null)
        {
            var ingredientes = await _context.Ingredientes
                .OrderBy(i => i.Nombre)
                .ToListAsync();

            ViewData["IdIngrediente"] =
                new SelectList(
                    ingredientes,
                    "IdIngrediente",
                    "Nombre",
                    ingredienteSeleccionado
                );
        }
    }
}