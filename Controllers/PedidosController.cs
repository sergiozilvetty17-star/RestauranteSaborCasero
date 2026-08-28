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
                // ==========================================
                // VALIDAR MESA
                // ==========================================

                // Si el pedido no utiliza mesa,
                // dejamos IdMesa como null.
                if (pedido.TipoPedido != TipoPedido.Mesa)
                {
                    pedido.IdMesa = null;
                }

                // ==========================================
                // FECHA Y HORA AUTOMÁTICAS
                // ==========================================

                if (pedido.Fecha == default)
                {
                    pedido.Fecha = DateTime.Now.Date;
                }

                if (pedido.HoraInicio == default)
                {
                    pedido.HoraInicio = DateTime.Now.TimeOfDay;
                }

                // ==========================================
                // ESTADO INICIAL
                // ==========================================

                // Todo pedido nuevo comienza como Pendiente.
                pedido.Estado = EstadoPedido.Pendiente;

                // Al crear el pedido todavía no existe
                // una hora de finalización.
                pedido.HoraFin = null;
                pedido.HoraEnPreparacion = null;
                pedido.HoraListo = null;
                pedido.HoraEntregado = null;
                pedido.HoraCancelado = null;

                // ==========================================
                // GUARDAR PEDIDO
                // ==========================================

                _context.Pedidos.Add(pedido);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Si existen errores de validación,
            // volvemos a cargar las listas.
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
                    // ==========================================
                    // VALIDAR MESA
                    // ==========================================

                    if (pedido.TipoPedido != TipoPedido.Mesa)
                    {
                        pedido.IdMesa = null;
                    }

                    // ==========================================
                    // ACTUALIZAR PEDIDO
                    // ==========================================

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
        // POST: Pedidos/CambiarEstado
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(
            int id,
            EstadoPedido nuevoEstado)
        {
            // ==========================================
            // BUSCAR PEDIDO
            // ==========================================

            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            // ==========================================
            // HORA ACTUAL
            // ==========================================

            var horaActual = DateTime.Now.TimeOfDay;

            // ==========================================
            // CAMBIAR ESTADO
            // ==========================================

            pedido.Estado = nuevoEstado;

            // ==========================================
            // GUARDAR HORA SEGÚN EL ESTADO
            // ==========================================

            switch (nuevoEstado)
            {
                case EstadoPedido.Pendiente:

                    // Si por alguna razón el pedido
                    // no tiene hora de inicio,
                    // la registramos ahora.

                    if (pedido.HoraInicio == default)
                    {
                        pedido.HoraInicio = horaActual;
                    }

                    break;


                case EstadoPedido.EnPreparacion:

                    // Registramos la hora en que
                    // comenzó la preparación.

                    if (pedido.HoraEnPreparacion == null)
                    {
                        pedido.HoraEnPreparacion = horaActual;
                    }

                    break;


                case EstadoPedido.Listo:

                    // Registramos la hora en que
                    // el pedido quedó listo.

                    if (pedido.HoraListo == null)
                    {
                        pedido.HoraListo = horaActual;
                    }

                    break;


                case EstadoPedido.Entregado:

                    // Registramos la hora de entrega.

                    if (pedido.HoraEntregado == null)
                    {
                        pedido.HoraEntregado = horaActual;
                    }

                    // Al entregar el pedido,
                    // también registramos la hora de finalización.

                    pedido.HoraFin = horaActual;

                    break;


                case EstadoPedido.Cancelado:

                    // Registramos la hora de cancelación.

                    if (pedido.HoraCancelado == null)
                    {
                        pedido.HoraCancelado = horaActual;
                    }

                    // Un pedido cancelado también
                    // se considera finalizado.

                    pedido.HoraFin = horaActual;

                    break;
            }

            // ==========================================
            // GUARDAR CAMBIOS
            // ==========================================

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // MÉTODO PARA CARGAR LISTAS
        // ==========================================

        private async Task CargarListas(
            int? meseroSeleccionado = null,
            int? mesaSeleccionada = null)
        {
            // ==========================================
            // USUARIOS QUE PUEDEN SER MESEROS
            // ==========================================

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

            // ==========================================
            // MESAS DISPONIBLES
            // ==========================================

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