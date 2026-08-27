using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Data;
using RestauranteSaborCasero.Models;

namespace RestauranteSaborCasero.Controllers
{
    public class PlatoIngredientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlatoIngredientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET: PlatoIngredientes
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var relaciones = await _context.PlatoIngredientes
                .Include(pi => pi.Plato)
                .Include(pi => pi.Ingrediente)
                .OrderBy(pi => pi.Plato.Nombre)
                .ThenBy(pi => pi.Ingrediente.Nombre)
                .ToListAsync();

            return View(relaciones);
        }


        // ==========================================
        // GET: PlatoIngredientes/Create
        // ==========================================

        public async Task<IActionResult> Create()
        {
            await CargarListas();

            return View();
        }


        // ==========================================
        // POST: PlatoIngredientes/Create
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlatoIngrediente platoIngrediente)
        {
            // Verificar que no exista ya la combinación
            // Plato + Ingrediente.
            bool existe = await _context.PlatoIngredientes
                .AnyAsync(pi =>
                    pi.IdPlato == platoIngrediente.IdPlato &&
                    pi.IdIngrediente == platoIngrediente.IdIngrediente);

            if (existe)
            {
                ModelState.AddModelError(
                    "",
                    "Este ingrediente ya está asignado a este plato."
                );
            }

            if (ModelState.IsValid)
            {
                _context.Add(platoIngrediente);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await CargarListas(
                platoIngrediente.IdPlato,
                platoIngrediente.IdIngrediente
            );

            return View(platoIngrediente);
        }


        // ==========================================
        // GET: PlatoIngredientes/Edit
        // ==========================================

        public async Task<IActionResult> Edit(
            int? idPlato,
            int? idIngrediente)
        {
            if (idPlato == null || idIngrediente == null)
            {
                return NotFound();
            }

            var platoIngrediente = await _context.PlatoIngredientes
                .FirstOrDefaultAsync(pi =>
                    pi.IdPlato == idPlato &&
                    pi.IdIngrediente == idIngrediente);

            if (platoIngrediente == null)
            {
                return NotFound();
            }

            await CargarListas(
                platoIngrediente.IdPlato,
                platoIngrediente.IdIngrediente
            );

            return View(platoIngrediente);
        }


        // ==========================================
        // POST: PlatoIngredientes/Edit
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int idPlato,
            int idIngrediente,
            PlatoIngrediente platoIngrediente)
        {
            if (idPlato != platoIngrediente.IdPlato ||
                idIngrediente != platoIngrediente.IdIngrediente)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(platoIngrediente);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    bool existe = await _context.PlatoIngredientes
                        .AnyAsync(pi =>
                            pi.IdPlato == idPlato &&
                            pi.IdIngrediente == idIngrediente);

                    if (!existe)
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await CargarListas(
                platoIngrediente.IdPlato,
                platoIngrediente.IdIngrediente
            );

            return View(platoIngrediente);
        }


        // ==========================================
        // GET: PlatoIngredientes/Delete
        // ==========================================

        public async Task<IActionResult> Delete(
            int? idPlato,
            int? idIngrediente)
        {
            if (idPlato == null || idIngrediente == null)
            {
                return NotFound();
            }

            var platoIngrediente = await _context.PlatoIngredientes
                .Include(pi => pi.Plato)
                .Include(pi => pi.Ingrediente)
                .FirstOrDefaultAsync(pi =>
                    pi.IdPlato == idPlato &&
                    pi.IdIngrediente == idIngrediente);

            if (platoIngrediente == null)
            {
                return NotFound();
            }

            return View(platoIngrediente);
        }


        // ==========================================
        // POST: PlatoIngredientes/Delete
        // ==========================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int idPlato,
            int idIngrediente)
        {
            var platoIngrediente = await _context.PlatoIngredientes
                .FirstOrDefaultAsync(pi =>
                    pi.IdPlato == idPlato &&
                    pi.IdIngrediente == idIngrediente);

            if (platoIngrediente != null)
            {
                _context.PlatoIngredientes.Remove(platoIngrediente);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // LISTAS DESPLEGABLES
        // ==========================================

        private async Task CargarListas(
            int? platoSeleccionado = null,
            int? ingredienteSeleccionado = null)
        {
            ViewData["IdPlato"] = new SelectList(
                await _context.Platos
                    .OrderBy(p => p.Nombre)
                    .ToListAsync(),
                "IdPlato",
                "Nombre",
                platoSeleccionado
            );

            ViewData["IdIngrediente"] = new SelectList(
                await _context.Ingredientes
                    .OrderBy(i => i.Nombre)
                    .ToListAsync(),
                "IdIngrediente",
                "Nombre",
                ingredienteSeleccionado
            );
        }
    }
}