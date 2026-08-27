using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Models;
using RestauranteSaborCasero.Data;

namespace RestauranteSaborCasero.Controllers
{
    public class PlatosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlatosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Platos
        public async Task<IActionResult> Index()
        {
            var platos = await _context.Platos
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(platos);
        }

        // GET: Platos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var plato = await _context.Platos
                .FirstOrDefaultAsync(p => p.IdPlato == id);

            if (plato == null)
                return NotFound();

            return View(plato);
        }

        // GET: Platos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Platos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Plato plato)
        {
            if (ModelState.IsValid)
            {
                _context.Add(plato);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(plato);
        }

        // GET: Platos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var plato = await _context.Platos.FindAsync(id);

            if (plato == null)
                return NotFound();

            return View(plato);
        }

        // POST: Platos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Plato plato)
        {
            if (id != plato.IdPlato)
                return NotFound();

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

        // GET: Platos/Delete/5
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

        // POST: Platos/Delete/5
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