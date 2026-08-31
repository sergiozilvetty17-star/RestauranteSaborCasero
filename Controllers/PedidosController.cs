using System.Security.Claims;
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
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
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
            // ==========================================
            // OBTENER USUARIO DE LA SESIÓN
            // ==========================================

            var idUsuarioClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrEmpty(idUsuarioClaim))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return RedirectToAction("Index", "Login");
            }


            // ==========================================
            // BUSCAR USUARIO
            // ==========================================

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == idUsuario);

            if (usuario == null || !usuario.Activo)
            {
                return RedirectToAction("Index", "Login");
            }


            // ==========================================
            // VERIFICAR MESERO
            // ==========================================

            if (usuario.Rol != RolUsuario.Mesero)
            {
                TempData["Error"] =
                    "Solo los usuarios con rol Mesero pueden registrar pedidos.";

                return RedirectToAction(nameof(Index));
            }


            // ==========================================
            // DATOS DEL MESERO
            // ==========================================

            ViewData["NombreMesero"] = usuario.Nombre;


            // ==========================================
            // CARGAR MESAS Y PLATOS
            // ==========================================

            await CargarListas();


            // ==========================================
            // CREAR PEDIDO VACÍO
            // ==========================================

            var pedido = new Pedido
            {
                Fecha = DateTime.Now.Date,
                HoraInicio = DateTime.Now.TimeOfDay,
                Estado = EstadoPedido.Pendiente,

                Detalles = new List<DetallePedido>()
            };


            return View(pedido);
        }


        // ==========================================
        // POST: Pedidos/Create
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pedido pedido)
        {
            // ==========================================
            // OBTENER USUARIO DE LA SESIÓN
            // ==========================================

            var idUsuarioClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrEmpty(idUsuarioClaim))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return RedirectToAction("Index", "Login");
            }


            // ==========================================
            // BUSCAR USUARIO
            // ==========================================

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == idUsuario);

            if (usuario == null || !usuario.Activo)
            {
                return RedirectToAction("Index", "Login");
            }


            // ==========================================
            // VERIFICAR MESERO
            // ==========================================

            if (usuario.Rol != RolUsuario.Mesero)
            {
                TempData["Error"] =
                    "Solo los usuarios con rol Mesero pueden registrar pedidos.";

                return RedirectToAction(nameof(Index));
            }


            // ==========================================
            // ASIGNAR MESERO AUTOMÁTICAMENTE
            // ==========================================

            pedido.IdMesero = usuario.IdUsuario;


            // ==========================================
            // FECHA Y HORA AUTOMÁTICAS
            // ==========================================

            pedido.Fecha = DateTime.Now.Date;

            pedido.HoraInicio = DateTime.Now.TimeOfDay;


            // ==========================================
            // ESTADO INICIAL
            // ==========================================

            pedido.Estado = EstadoPedido.Pendiente;

            pedido.HoraFin = null;
            pedido.HoraEnPreparacion = null;
            pedido.HoraListo = null;
            pedido.HoraEntregado = null;
            pedido.HoraCancelado = null;


            // ==========================================
            // VALIDAR TIPO DE PEDIDO
            // ==========================================

            if (pedido.TipoPedido != TipoPedido.Mesa)
            {
                pedido.IdMesa = null;
            }


            // ==========================================
            // VALIDAR DETALLES
            // ==========================================

            if (pedido.Detalles == null ||
                !pedido.Detalles.Any())
            {
                ModelState.AddModelError(
                    "Detalles",
                    "Debes agregar al menos un plato al pedido."
                );
            }


            // ==========================================
            // QUITAR VALIDACIONES DE CAMPOS AUTOMÁTICOS
            // ==========================================

            ModelState.Remove(nameof(Pedido.IdMesero));
            ModelState.Remove(nameof(Pedido.Fecha));
            ModelState.Remove(nameof(Pedido.HoraInicio));
            ModelState.Remove(nameof(Pedido.Estado));


            // ==========================================
            // VALIDAR LOS PLATOS
            // ==========================================

            if (pedido.Detalles != null &&
                pedido.Detalles.Any())
            {
                foreach (var detalle in pedido.Detalles)
                {
                    // Validar que el plato exista
                    var platoExiste = await _context.Platos
                        .AnyAsync(p =>
                            p.IdPlato == detalle.IdPlato &&
                            p.Estado == EstadoPlato.Disponible);

                    if (!platoExiste)
                    {
                        ModelState.AddModelError(
                            "Detalles",
                            "Uno de los platos seleccionados no existe o no está disponible."
                        );
                    }


                    // Validar cantidad
                    if (detalle.Cantidad < 1)
                    {
                        ModelState.AddModelError(
                            "Detalles",
                            "La cantidad de cada plato debe ser mayor a 0."
                        );
                    }
                }
            }


            // ==========================================
            // GUARDAR PEDIDO
            // ==========================================

            if (ModelState.IsValid)
            {
                try
                {
                    // ==========================================
                    // IMPORTANTE:
                    // Entity Framework relacionará automáticamente
                    // los detalles con el pedido.
                    // ==========================================

                    foreach (var detalle in pedido.Detalles)
                    {
                        detalle.Pedido = pedido;
                    }


                    // Agregar pedido
                    _context.Pedidos.Add(pedido);


                    // Guardar pedido + detalles
                    await _context.SaveChangesAsync();


                    // ==========================================
                    // MENSAJE DE ÉXITO
                    // ==========================================

                    TempData["Success"] =
                        "El pedido se registró correctamente.";


                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        "Ocurrió un error al guardar el pedido: "
                        + ex.Message
                    );
                }
            }


            // ==========================================
            // SI HAY ERRORES
            // ==========================================

            await CargarListas(pedido.IdMesa);

            ViewData["NombreMesero"] = usuario.Nombre;

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
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .FirstOrDefaultAsync(p =>
                    p.IdPedido == id);

            if (pedido == null)
                return NotFound();


            // ==========================================
            // OBTENER MESERO
            // ==========================================

            var mesero = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == pedido.IdMesero);


            ViewData["NombreMesero"] =
                mesero?.Nombre ?? "Sin mesero";


            // ==========================================
            // CARGAR LISTAS
            // ==========================================

            await CargarListas(pedido.IdMesa);


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


            // ==========================================
            // BUSCAR PEDIDO ORIGINAL
            // ==========================================

            var pedidoOriginal = await _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p =>
                    p.IdPedido == id);

            if (pedidoOriginal == null)
                return NotFound();


            // ==========================================
            // CONSERVAR MESERO
            // ==========================================

            pedidoOriginal.IdMesero =
                pedidoOriginal.IdMesero;


            // ==========================================
            // VALIDAR MESA
            // ==========================================

            if (pedido.TipoPedido != TipoPedido.Mesa)
            {
                pedido.IdMesa = null;
            }


            // ==========================================
            // QUITAR VALIDACIÓN DEL MESERO
            // ==========================================

            ModelState.Remove(nameof(Pedido.IdMesero));


            // ==========================================
            // ACTUALIZAR PEDIDO
            // ==========================================

            if (ModelState.IsValid)
            {
                try
                {
                    pedidoOriginal.IdMesa =
                        pedido.IdMesa;

                    pedidoOriginal.TipoPedido =
                        pedido.TipoPedido;


                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Pedidos
                        .AnyAsync(p =>
                            p.IdPedido == id))
                    {
                        return NotFound();
                    }

                    throw;
                }


                return RedirectToAction(nameof(Index));
            }


            // ==========================================
            // SI HAY ERRORES
            // ==========================================

            await CargarListas(pedido.IdMesa);


            var mesero = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == pedido.IdMesero);


            ViewData["NombreMesero"] =
                mesero?.Nombre ?? "Sin mesero";


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
                .FirstOrDefaultAsync(p =>
                    p.IdPedido == id);

            if (pedido == null)
                return NotFound();


            // ==========================================
            // HORA ACTUAL
            // ==========================================

            var horaActual =
                DateTime.Now.TimeOfDay;


            // ==========================================
            // CAMBIAR ESTADO
            // ==========================================

            pedido.Estado =
                nuevoEstado;


            // ==========================================
            // REGISTRAR HORAS
            // ==========================================

            switch (nuevoEstado)
            {
                case EstadoPedido.Pendiente:

                    if (pedido.HoraInicio == default)
                    {
                        pedido.HoraInicio =
                            horaActual;
                    }

                    break;


                case EstadoPedido.EnPreparacion:

                    if (pedido.HoraEnPreparacion == null)
                    {
                        pedido.HoraEnPreparacion =
                            horaActual;
                    }

                    break;


                case EstadoPedido.Listo:

                    if (pedido.HoraListo == null)
                    {
                        pedido.HoraListo =
                            horaActual;
                    }

                    break;


                case EstadoPedido.Entregado:

                    if (pedido.HoraEntregado == null)
                    {
                        pedido.HoraEntregado =
                            horaActual;
                    }

                    pedido.HoraFin =
                        horaActual;

                    break;


                case EstadoPedido.Cancelado:

                    if (pedido.HoraCancelado == null)
                    {
                        pedido.HoraCancelado =
                            horaActual;
                    }

                    pedido.HoraFin =
                        horaActual;

                    break;
            }


            // ==========================================
            // GUARDAR
            // ==========================================

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // CARGAR MESAS Y PLATOS
        // ==========================================

        private async Task CargarListas(
            int? mesaSeleccionada = null)
        {
            // ==========================================
            // MESAS DISPONIBLES
            // ==========================================

            var mesas = await _context.Mesas
                .Where(m =>
                    m.Estado == EstadoMesa.Disponible ||
                    m.IdMesa == mesaSeleccionada)
                .OrderBy(m => m.NumeroMesa)
                .ToListAsync();


            ViewData["IdMesa"] =
                new SelectList(
                    mesas,
                    "IdMesa",
                    "NumeroMesa",
                    mesaSeleccionada
                );


            // ==========================================
            // PLATOS DISPONIBLES
            // ==========================================

            var platos = await _context.Platos
                .Where(p =>
                    p.Estado == EstadoPlato.Disponible)
                .OrderBy(p => p.Nombre)
                .ToListAsync();


            ViewData["Platos"] =
                platos;
        }
    }
}