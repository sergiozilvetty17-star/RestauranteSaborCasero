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

        // ============================================================
        // GET: Pedidos
        // ============================================================

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

        // ============================================================
        // GET: Pedidos/Details/5
        // ============================================================

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

        // ============================================================
        // GET: Pedidos/Create
        // ============================================================

        public async Task<IActionResult> Create()
        {
            var usuario = await ObtenerUsuarioActual();

            if (usuario == null)
                return RedirectToAction("Index", "Login");

            if (usuario.Rol != RolUsuario.Mesero)
            {
                TempData["Error"] =
                    "Solo los usuarios con rol Mesero pueden registrar pedidos.";

                return RedirectToAction(nameof(Index));
            }

            ViewData["NombreMesero"] = usuario.Nombre;

            await CargarListas();

            var pedido = new Pedido
            {
                Fecha = DateTime.Now.Date,
                HoraInicio = DateTime.Now.TimeOfDay,
                Estado = EstadoPedido.Pendiente,
                TipoPedido = TipoPedido.Mesa
            };

            return View(pedido);
        }

        // ============================================================
        // POST: Pedidos/Create
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pedido pedido)
        {
            // ========================================================
            // OBTENER USUARIO ACTUAL
            // ========================================================

            var usuario = await ObtenerUsuarioActual();

            if (usuario == null)
                return RedirectToAction("Index", "Login");

            if (usuario.Rol != RolUsuario.Mesero)
            {
                TempData["Error"] =
                    "Solo los usuarios con rol Mesero pueden registrar pedidos.";

                return RedirectToAction(nameof(Index));
            }

            // ========================================================
            // ELIMINAR VALIDACIONES DE PROPIEDADES DE NAVEGACIÓN
            // ========================================================

            ModelState.Remove(nameof(Pedido.Usuario));
            ModelState.Remove(nameof(Pedido.Mesa));

            if (pedido.Detalles != null)
            {
                for (int i = 0; i < pedido.Detalles.Count; i++)
                {
                    ModelState.Remove($"Detalles[{i}].Pedido");
                    ModelState.Remove($"Detalles[{i}].Plato");
                }
            }

            // ========================================================
            // DATOS AUTOMÁTICOS
            // ========================================================

            pedido.IdMesero = usuario.IdUsuario;
            pedido.Fecha = DateTime.Now.Date;
            pedido.HoraInicio = DateTime.Now.TimeOfDay;

            pedido.Estado = EstadoPedido.Pendiente;

            pedido.HoraFin = null;
            pedido.HoraEnPreparacion = null;
            pedido.HoraListo = null;
            pedido.HoraEntregado = null;
            pedido.HoraCancelado = null;

            // ========================================================
            // VALIDAR MESERO
            // ========================================================

            var meseroExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.IdUsuario == usuario.IdUsuario &&
                    u.Activo);

            if (!meseroExiste)
            {
                ModelState.AddModelError(
                    "",
                    "El usuario que intenta registrar el pedido no existe o está inactivo."
                );
            }

            // ========================================================
            // VALIDAR TIPO DE PEDIDO
            // ========================================================

            if (pedido.TipoPedido == TipoPedido.ParaLlevar)
            {
                pedido.IdMesa = null;
            }
            else
            {
                if (!pedido.IdMesa.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(Pedido.IdMesa),
                        "Debes seleccionar una mesa."
                    );
                }
                else
                {
                    var mesa = await _context.Mesas
                        .FirstOrDefaultAsync(m =>
                            m.IdMesa == pedido.IdMesa.Value);

                    if (mesa == null)
                    {
                        ModelState.AddModelError(
                            nameof(Pedido.IdMesa),
                            "La mesa seleccionada no existe."
                        );
                    }
                    else if (mesa.Estado != EstadoMesa.Disponible)
                    {
                        ModelState.AddModelError(
                            nameof(Pedido.IdMesa),
                            "La mesa seleccionada no está disponible."
                        );
                    }
                }
            }

            // ========================================================
            // VALIDAR DETALLES
            // ========================================================

            if (pedido.Detalles == null ||
                !pedido.Detalles.Any())
            {
                ModelState.AddModelError(
                    "Detalles",
                    "Debes agregar al menos un plato al pedido."
                );
            }
            else
            {
                foreach (var detalle in pedido.Detalles)
                {
                    if (detalle.IdPlato <= 0)
                    {
                        ModelState.AddModelError(
                            "Detalles",
                            "Debes seleccionar un plato válido."
                        );

                        continue;
                    }

                    if (detalle.Cantidad < 1)
                    {
                        ModelState.AddModelError(
                            "Detalles",
                            "La cantidad debe ser mayor a 0."
                        );
                    }

                    if (detalle.Cantidad > 1000)
                    {
                        ModelState.AddModelError(
                            "Detalles",
                            "La cantidad no puede ser mayor a 1000."
                        );
                    }

                    var platoExiste = await _context.Platos
                        .AnyAsync(p =>
                            p.IdPlato == detalle.IdPlato &&
                            p.Estado == EstadoPlato.Disponible);

                    if (!platoExiste)
                    {
                        ModelState.AddModelError(
                            "Detalles",
                            $"El plato con ID {detalle.IdPlato} no existe o no está disponible."
                        );
                    }
                }
            }

            // ========================================================
            // QUITAR VALIDACIONES DE DATOS AUTOMÁTICOS
            // ========================================================

            ModelState.Remove(nameof(Pedido.IdMesero));
            ModelState.Remove(nameof(Pedido.Fecha));
            ModelState.Remove(nameof(Pedido.HoraInicio));
            ModelState.Remove(nameof(Pedido.Estado));
            ModelState.Remove(nameof(Pedido.HoraFin));
            ModelState.Remove(nameof(Pedido.HoraEnPreparacion));
            ModelState.Remove(nameof(Pedido.HoraListo));
            ModelState.Remove(nameof(Pedido.HoraEntregado));
            ModelState.Remove(nameof(Pedido.HoraCancelado));

            // ========================================================
            // MOSTRAR ERRORES REALES EN CONSOLA
            // ========================================================

            if (!ModelState.IsValid)
            {
                Console.WriteLine("");
                Console.WriteLine("==============================================");
                Console.WriteLine("ERRORES DE MODELO AL CREAR PEDIDO");
                Console.WriteLine("==============================================");

                foreach (var item in ModelState)
                {
                    foreach (var error in item.Value.Errors)
                    {
                        Console.WriteLine(
                            $"CAMPO: {item.Key} | ERROR: {error.ErrorMessage}"
                        );
                    }
                }

                Console.WriteLine("==============================================");
                Console.WriteLine("");

                await CargarListas(pedido.IdMesa);

                ViewData["NombreMesero"] = usuario.Nombre;

                return View(pedido);
            }

            // ========================================================
            // GUARDAR PEDIDO
            // ========================================================

            try
            {
                // ----------------------------------------------------
                // CREAR PEDIDO PRINCIPAL
                // ----------------------------------------------------

                var nuevoPedido = new Pedido
                {
                    IdMesero = usuario.IdUsuario,
                    IdMesa = pedido.IdMesa,
                    TipoPedido = pedido.TipoPedido,
                    Fecha = DateTime.Now.Date,
                    HoraInicio = DateTime.Now.TimeOfDay,
                    HoraFin = null,
                    HoraEnPreparacion = null,
                    HoraListo = null,
                    HoraEntregado = null,
                    HoraCancelado = null,
                    Estado = EstadoPedido.Pendiente
                };

                // ----------------------------------------------------
                // AGREGAR PEDIDO
                // ----------------------------------------------------

                _context.Pedidos.Add(nuevoPedido);

                // Primero guardamos para obtener IdPedido
                await _context.SaveChangesAsync();

                Console.WriteLine(
                    $"PEDIDO CREADO CORRECTAMENTE. ID: {nuevoPedido.IdPedido}"
                );

                // ----------------------------------------------------
                // GUARDAR DETALLES
                // ----------------------------------------------------

                foreach (var detalle in pedido.Detalles!)
                {
                    var nuevoDetalle = new DetallePedido
                    {
                        IdPedido = nuevoPedido.IdPedido,
                        IdPlato = detalle.IdPlato,
                        Cantidad = detalle.Cantidad,
                        IndicacionesExtra =
                            string.IsNullOrWhiteSpace(
                                detalle.IndicacionesExtra)
                            ? null
                            : detalle.IndicacionesExtra.Trim()
                    };

                    _context.DetallePedidos.Add(nuevoDetalle);
                }

                // ----------------------------------------------------
                // OCUPAR MESA
                // ----------------------------------------------------

                if (nuevoPedido.TipoPedido == TipoPedido.Mesa &&
                    nuevoPedido.IdMesa.HasValue)
                {
                    var mesa = await _context.Mesas
                        .FirstOrDefaultAsync(m =>
                            m.IdMesa == nuevoPedido.IdMesa.Value);

                    if (mesa != null)
                    {
                        mesa.Estado = EstadoMesa.Ocupada;

                        Console.WriteLine(
                            $"MESA {mesa.NumeroMesa} CAMBIADA A OCUPADA."
                        );
                    }
                }

                // ----------------------------------------------------
                // GUARDAR DETALLES Y MESA
                // ----------------------------------------------------

                await _context.SaveChangesAsync();

                Console.WriteLine(
                    $"DETALLES DEL PEDIDO {nuevoPedido.IdPedido} GUARDADOS."
                );

                TempData["Success"] =
                    $"El pedido #{nuevoPedido.IdPedido} se registró correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("");
                Console.WriteLine("==============================================");
                Console.WriteLine("ERROR DE BASE DE DATOS AL GUARDAR PEDIDO");
                Console.WriteLine("==============================================");
                Console.WriteLine(ex.Message);

                if (ex.InnerException != null)
                {
                    Console.WriteLine("INNER EXCEPTION:");
                    Console.WriteLine(ex.InnerException.Message);
                }

                Console.WriteLine("==============================================");

                ModelState.AddModelError(
                    "",
                    "No se pudo guardar el pedido en la base de datos. " +
                    "Revisa la consola para ver el error."
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("==============================================");
                Console.WriteLine("ERROR GENERAL AL GUARDAR PEDIDO");
                Console.WriteLine("==============================================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("==============================================");

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error inesperado al guardar el pedido."
                );
            }

            // ========================================================
            // VOLVER AL FORMULARIO
            // ========================================================

            await CargarListas(pedido.IdMesa);

            ViewData["NombreMesero"] = usuario.Nombre;

            return View(pedido);
        }

        // ============================================================
        // GET: Pedidos/Edit/5
        // ============================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .Include(p => p.Mesa)
                .FirstOrDefaultAsync(p =>
                    p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            if (pedido.Estado == EstadoPedido.Entregado ||
                pedido.Estado == EstadoPedido.Cancelado)
            {
                TempData["Error"] =
                    "No se puede editar un pedido entregado o cancelado.";

                return RedirectToAction(nameof(Index));
            }

            var mesero = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == pedido.IdMesero);

            ViewData["NombreMesero"] =
                mesero?.Nombre ?? "Sin mesero";

            await CargarListas(pedido.IdMesa);

            return View(pedido);
        }

        // ============================================================
        // POST: Pedidos/Edit/5
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Pedido pedido)
        {
            if (id != pedido.IdPedido)
                return NotFound();

            var pedidoOriginal = await _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p =>
                    p.IdPedido == id);

            if (pedidoOriginal == null)
                return NotFound();

            if (pedidoOriginal.Estado == EstadoPedido.Entregado ||
                pedidoOriginal.Estado == EstadoPedido.Cancelado)
            {
                TempData["Error"] =
                    "No se puede modificar un pedido entregado o cancelado.";

                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove(nameof(Pedido.Usuario));
            ModelState.Remove(nameof(Pedido.Mesa));

            if (pedido.Detalles != null)
            {
                for (int i = 0; i < pedido.Detalles.Count; i++)
                {
                    ModelState.Remove($"Detalles[{i}].Pedido");
                    ModelState.Remove($"Detalles[{i}].Plato");
                }
            }

            if (pedido.TipoPedido == TipoPedido.ParaLlevar)
            {
                pedido.IdMesa = null;
            }
            else
            {
                if (!pedido.IdMesa.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(Pedido.IdMesa),
                        "Debes seleccionar una mesa."
                    );
                }
                else
                {
                    var mesa = await _context.Mesas
                        .FirstOrDefaultAsync(m =>
                            m.IdMesa == pedido.IdMesa);

                    if (mesa == null)
                    {
                        ModelState.AddModelError(
                            nameof(Pedido.IdMesa),
                            "La mesa seleccionada no existe."
                        );
                    }
                }
            }

            ModelState.Remove(nameof(Pedido.IdMesero));
            ModelState.Remove(nameof(Pedido.Fecha));
            ModelState.Remove(nameof(Pedido.HoraInicio));
            ModelState.Remove(nameof(Pedido.Estado));
            ModelState.Remove(nameof(Pedido.HoraFin));
            ModelState.Remove(nameof(Pedido.HoraEnPreparacion));
            ModelState.Remove(nameof(Pedido.HoraListo));
            ModelState.Remove(nameof(Pedido.HoraEntregado));
            ModelState.Remove(nameof(Pedido.HoraCancelado));

            if (ModelState.IsValid)
            {
                try
                {
                    var mesaAnterior = pedidoOriginal.IdMesa;

                    pedidoOriginal.TipoPedido =
                        pedido.TipoPedido;

                    pedidoOriginal.IdMesa =
                        pedido.IdMesa;

                    if (mesaAnterior.HasValue &&
                        mesaAnterior != pedido.IdMesa)
                    {
                        var mesaAnteriorDb =
                            await _context.Mesas
                                .FirstOrDefaultAsync(m =>
                                    m.IdMesa == mesaAnterior.Value);

                        if (mesaAnteriorDb != null)
                        {
                            mesaAnteriorDb.Estado =
                                EstadoMesa.Disponible;
                        }
                    }

                    if (pedido.TipoPedido == TipoPedido.Mesa &&
                        pedido.IdMesa.HasValue)
                    {
                        var nuevaMesa =
                            await _context.Mesas
                                .FirstOrDefaultAsync(m =>
                                    m.IdMesa == pedido.IdMesa.Value);

                        if (nuevaMesa != null)
                        {
                            nuevaMesa.Estado =
                                EstadoMesa.Ocupada;
                        }
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        "El pedido se actualizó correctamente.";

                    return RedirectToAction(nameof(Index));
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                    ModelState.AddModelError(
                        "",
                        "Ocurrió un error al actualizar el pedido."
                    );
                }
            }

            await CargarListas(pedido.IdMesa);

            var mesero = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == pedidoOriginal.IdMesero);

            ViewData["NombreMesero"] =
                mesero?.Nombre ?? "Sin mesero";

            return View(pedido);
        }

        // ============================================================
        // POST: Pedidos/CambiarEstado
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(
            int id,
            EstadoPedido nuevoEstado)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Mesa)
                .FirstOrDefaultAsync(p =>
                    p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            if (pedido.Estado == EstadoPedido.Entregado ||
                pedido.Estado == EstadoPedido.Cancelado)
            {
                TempData["Error"] =
                    "El pedido ya está finalizado y no puede cambiar de estado.";

                return RedirectToAction(nameof(Index));
            }

            if (!EsTransicionValida(
                    pedido.Estado,
                    nuevoEstado))
            {
                TempData["Error"] =
                    $"No se puede cambiar el pedido de " +
                    $"{pedido.Estado} a {nuevoEstado}.";

                return RedirectToAction(nameof(Index));
            }

            var horaActual =
                DateTime.Now.TimeOfDay;

            pedido.Estado = nuevoEstado;

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

                    if (pedido.IdMesa.HasValue)
                    {
                        var mesa = await _context.Mesas
                            .FirstOrDefaultAsync(m =>
                                m.IdMesa == pedido.IdMesa.Value);

                        if (mesa != null)
                        {
                            mesa.Estado =
                                EstadoMesa.Disponible;
                        }
                    }

                    break;

                case EstadoPedido.Cancelado:

                    if (pedido.HoraCancelado == null)
                    {
                        pedido.HoraCancelado =
                            horaActual;
                    }

                    pedido.HoraFin =
                        horaActual;

                    if (pedido.IdMesa.HasValue)
                    {
                        var mesa = await _context.Mesas
                            .FirstOrDefaultAsync(m =>
                                m.IdMesa == pedido.IdMesa.Value);

                        if (mesa != null)
                        {
                            mesa.Estado =
                                EstadoMesa.Disponible;
                        }
                    }

                    break;
            }

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "El estado del pedido se actualizó correctamente.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                TempData["Error"] =
                    "Ocurrió un error al cambiar el estado.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // VALIDAR TRANSICIONES
        // ============================================================

        private bool EsTransicionValida(
            EstadoPedido estadoActual,
            EstadoPedido nuevoEstado)
        {
            if (estadoActual == nuevoEstado)
                return false;

            switch (estadoActual)
            {
                case EstadoPedido.Pendiente:

                    return nuevoEstado ==
                               EstadoPedido.EnPreparacion
                           || nuevoEstado ==
                               EstadoPedido.Cancelado;

                case EstadoPedido.EnPreparacion:

                    return nuevoEstado ==
                               EstadoPedido.Listo
                           || nuevoEstado ==
                               EstadoPedido.Cancelado;

                case EstadoPedido.Listo:

                    return nuevoEstado ==
                               EstadoPedido.Entregado
                           || nuevoEstado ==
                               EstadoPedido.Cancelado;

                case EstadoPedido.Entregado:
                case EstadoPedido.Cancelado:

                    return false;

                default:

                    return false;
            }
        }

        // ============================================================
        // OBTENER USUARIO ACTUAL
        // ============================================================

        private async Task<Usuario?> ObtenerUsuarioActual()
        {
            var idUsuarioClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(idUsuarioClaim))
                return null;

            if (!int.TryParse(
                    idUsuarioClaim,
                    out int idUsuario))
            {
                return null;
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == idUsuario);

            if (usuario == null ||
                !usuario.Activo)
            {
                return null;
            }

            return usuario;
        }

        // ============================================================
        // CARGAR MESAS Y PLATOS
        // ============================================================

        private async Task CargarListas(
            int? mesaSeleccionada = null)
        {
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
