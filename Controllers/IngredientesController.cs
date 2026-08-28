using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Models;
using RestauranteSaborCasero.Data;

namespace RestauranteSaborCasero.Controllers
{
    [Authorize(Roles = "Administrador,Cocinero")]
    public class IngredientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IngredientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ingredientes
        public async Task<IActionResult> Index()
        {
            var ingredientes = await _context.Ingredientes
                .OrderBy(i => i.Nombre)
                .ToListAsync();

            return View(ingredientes);
        }

        // GET: Ingredientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var ingrediente = await _context.Ingredientes
                .FirstOrDefaultAsync(i => i.IdIngrediente == id);

            if (ingrediente == null)
                return NotFound();

            return View(ingrediente);
        }

        // GET: Ingredientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ingredientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ingrediente ingrediente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ingrediente);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(ingrediente);
        }

        // GET: Ingredientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var ingrediente = await _context.Ingredientes.FindAsync(id);

            if (ingrediente == null)
                return NotFound();

            return View(ingrediente);
        }

        // POST: Ingredientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Ingrediente ingrediente)
        {
            if (id != ingrediente.IdIngrediente)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ingrediente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Ingredientes
                        .AnyAsync(i => i.IdIngrediente == id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(ingrediente);
        }

        // GET: Ingredientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var ingrediente = await _context.Ingredientes
                .FirstOrDefaultAsync(i => i.IdIngrediente == id);

            if (ingrediente == null)
                return NotFound();

            return View(ingrediente);
        }

        // POST: Ingredientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ingrediente = await _context.Ingredientes
                .FindAsync(id);

            if (ingrediente != null)
            {
                _context.Ingredientes.Remove(ingrediente);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}