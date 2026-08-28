using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Data;
using RestauranteSaborCasero.Models;

namespace RestauranteSaborCasero.Controllers
{
    [Authorize(Roles = "Administrador,Mesero")]
    public class MesasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MesasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET: Mesas
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var mesas = await _context.Mesas
                .OrderBy(m => m.NumeroMesa)
                .ToListAsync();

            return View(mesas);
        }


        // ==========================================
        // GET: Mesas/Details/5
        // ==========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mesa = await _context.Mesas
                .FirstOrDefaultAsync(m => m.IdMesa == id);

            if (mesa == null)
            {
                return NotFound();
            }

            return View(mesa);
        }


        // ==========================================
        // GET: Mesas/Create
        // ==========================================

        public IActionResult Create()
        {
            return View();
        }


        // ==========================================
        // POST: Mesas/Create
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mesa mesa)
        {
            // Verificar si el número de mesa ya existe.
            bool numeroExiste = await _context.Mesas
                .AnyAsync(m => m.NumeroMesa == mesa.NumeroMesa);

            if (numeroExiste)
            {
                ModelState.AddModelError(
                    "NumeroMesa",
                    "Ya existe una mesa con este número."
                );
            }

            if (ModelState.IsValid)
            {
                _context.Add(mesa);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(mesa);
        }


        // ==========================================
        // GET: Mesas/Edit/5
        // ==========================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mesa = await _context.Mesas
                .FindAsync(id);

            if (mesa == null)
            {
                return NotFound();
            }

            return View(mesa);
        }


        // ==========================================
        // POST: Mesas/Edit/5
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Mesa mesa)
        {
            if (id != mesa.IdMesa)
            {
                return NotFound();
            }

            // Verificar número de mesa duplicado.
            bool numeroExiste = await _context.Mesas
                .AnyAsync(m =>
                    m.NumeroMesa == mesa.NumeroMesa &&
                    m.IdMesa != mesa.IdMesa
                );

            if (numeroExiste)
            {
                ModelState.AddModelError(
                    "NumeroMesa",
                    "Ya existe otra mesa con este número."
                );
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mesa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MesaExiste(mesa.IdMesa))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(mesa);
        }


        // ==========================================
        // GET: Mesas/Delete/5
        // ==========================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mesa = await _context.Mesas
                .FirstOrDefaultAsync(m => m.IdMesa == id);

            if (mesa == null)
            {
                return NotFound();
            }

            return View(mesa);
        }


        // ==========================================
        // POST: Mesas/Delete/5
        // ==========================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mesa = await _context.Mesas
                .FindAsync(id);

            if (mesa != null)
            {
                _context.Mesas.Remove(mesa);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // MÉTODO AUXILIAR
        // ==========================================

        private bool MesaExiste(int id)
        {
            return _context.Mesas
                .Any(e => e.IdMesa == id);
        }
    }
}