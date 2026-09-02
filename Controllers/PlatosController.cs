using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Models;
using RestauranteSaborCasero.Data;

namespace RestauranteSaborCasero.Controllers
{
    [Authorize(Roles = "Administrador,Cocinero")]
    public class PlatosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlatosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Incluimos la receta para que esté disponible si la necesitas en la vista
            var platos = await _context.Platos
                .Include(p => p.PlatoIngredientes)
                    .ThenInclude(pi => pi.Ingrediente)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(platos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var plato = await _context.Platos
                .Include(p => p.PlatoIngredientes)
                    .ThenInclude(pi => pi.Ingrediente)
                .FirstOrDefaultAsync(p => p.IdPlato == id);

            if (plato == null)
                return NotFound();

            return View(plato);
        }

        // ======================================================
        // CREATE - GET
        // ======================================================
        public IActionResult Create()
        {
            // Cargamos todos los ingredientes disponibles para el formulario dinámico
            ViewBag.Ingredientes = new SelectList(_context.Ingredientes.OrderBy(i => i.Nombre), "IdIngrediente", "Nombre");

            return View();
        }

        // ======================================================
        // CREATE - POST
        // ======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Plato plato, int[] IdsIngredientes, decimal[] CantidadesNecesarias)
        {
            // Evitamos que valide estas colecciones que se llenarán manualmente
            ModelState.Remove("PlatoIngredientes");
            ModelState.Remove("DetallesPedido");

            if (ModelState.IsValid)
            {
                // Vinculamos los ingredientes que el usuario agregó en la vista (La Receta)
                if (IdsIngredientes != null && CantidadesNecesarias != null && IdsIngredientes.Length == CantidadesNecesarias.Length)
                {
                    for (int i = 0; i < IdsIngredientes.Length; i++)
                    {
                        plato.PlatoIngredientes.Add(new PlatoIngrediente
                        {
                            IdIngrediente = IdsIngredientes[i],
                            CantidadNecesaria = CantidadesNecesarias[i]
                        });
                    }
                }

                _context.Add(plato);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Si hay un error, recargamos la lista para que el select siga funcionando
            ViewBag.Ingredientes = new SelectList(_context.Ingredientes.OrderBy(i => i.Nombre), "IdIngrediente", "Nombre");

            return View(plato);
        }

        // ======================================================
        // EDIT - GET
        // ======================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var plato = await _context.Platos.FindAsync(id);

            if (plato == null)
                return NotFound();

            return View(plato);
        }

        // ======================================================
        // EDIT - POST
        // ======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Plato plato)
        {
            if (id != plato.IdPlato)
                return NotFound();

            // Quitamos validaciones de listas para evitar errores en la edición básica
            ModelState.Remove("PlatoIngredientes");
            ModelState.Remove("DetallesPedido");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plato);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Platos.AnyAsync(p => p.IdPlato == id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(plato);
        }

        // ======================================================
        // DELETE - GET
        // ======================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var plato = await _context.Platos
                .FirstOrDefaultAsync(p => p.IdPlato == id);

            if (plato == null)
                return NotFound();

            return View(plato);
        }

        // ======================================================
        // DELETE - POST
        // ======================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plato = await _context.Platos.FindAsync(id);

            if (plato != null)
            {
                _context.Platos.Remove(plato);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}