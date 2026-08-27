using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Data;
using RestauranteSaborCasero.Models;

namespace RestauranteSaborCasero.Controllers
{
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PedidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET: Pedidos
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Mesa)
                .OrderByDescending(p => p.Fecha)
                .ThenByDescending(p => p.HoraInicio)
                .ToListAsync();

            return View(pedidos);
        }


        // ==========================================
        // GET: Pedidos/Details/5
        // ==========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            return View(pedido);
        }


        // ==========================================
        // GET: Pedidos/Create
        // ==========================================

        public async Task<IActionResult> Create()
        {
            await CargarListas();

            return View();
        }


        // ==========================================
        // POST: Pedidos/Create
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pedido pedido)
        {
            if (ModelState.IsValid)
            {
                // Si el pedido no utiliza mesa,
                // dejamos IdMesa como null.
                if (pedido.TipoPedido != TipoPedido.Mesa)
                {
                    pedido.IdMesa = null;
                }

                // Fecha y hora de inicio automáticas
                if (pedido.Fecha == default)
                {
                    pedido.Fecha = DateTime.Now.Date;
                }

                if (pedido.HoraInicio == default)
                {
                    pedido.HoraInicio = DateTime.Now.TimeOfDay;
                }

                _context.Pedidos.Add(pedido);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await CargarListas(
                pedido.IdMesero,
                pedido.IdMesa
            );

            return View(pedido);
        }


        // ==========================================
        // GET: Pedidos/Edit/5
        // ==========================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var pedido = await _context.Pedidos
                .FindAsync(id);

            if (pedido == null)
                return NotFound();

            await CargarListas(
                pedido.IdMesero,
                pedido.IdMesa
            );

            return View(pedido);
        }


        // ==========================================
        // POST: Pedidos/Edit/5
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Pedido pedido)
        {
            if (id != pedido.IdPedido)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (pedido.TipoPedido != TipoPedido.Mesa)
                    {
                        pedido.IdMesa = null;
                    }

                    _context.Update(pedido);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Pedidos
                        .AnyAsync(p => p.IdPedido == id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await CargarListas(
                pedido.IdMesero,
                pedido.IdMesa
            );

            return View(pedido);
        }


        // ==========================================
        // MÉTODO PARA CARGAR LISTAS
        // ==========================================

        private async Task CargarListas(
            int? meseroSeleccionado = null,
            int? mesaSeleccionada = null)
        {
            // Usuarios que pueden ser meseros
            var meseros = await _context.Usuarios
                .Where(u =>
                    u.Rol == RolUsuario.Mesero &&
                    u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            ViewData["IdMesero"] = new SelectList(
                meseros,
                "IdUsuario",
                "Nombre",
                meseroSeleccionado
            );

            // Mesas disponibles
            var mesas = await _context.Mesas
                .Where(m =>
                    m.Estado == EstadoMesa.Disponible ||
                    m.IdMesa == mesaSeleccionada)
                .OrderBy(m => m.NumeroMesa)
                .ToListAsync();

            ViewData["IdMesa"] = new SelectList(
                mesas,
                "IdMesa",
                "NumeroMesa",
                mesaSeleccionada
            );
        }
    }
}