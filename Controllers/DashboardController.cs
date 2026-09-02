using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Data;
using RestauranteSaborCasero.Models;

namespace RestauranteSaborCasero.Controllers
{
[Authorize]
public class DashboardController : Controller
{
private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string periodo = "hoy",
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null)
    {
        DateTime hoy = DateTime.Today;

        DateTime desde;
        DateTime hasta;

        switch (periodo?.ToLower())
        {
            case "ayer":
                desde = hoy.AddDays(-1);
                hasta = hoy.AddDays(-1);
                break;

            case "7dias":
                desde = hoy.AddDays(-6);
                hasta = hoy;
                break;

            case "30dias":
                desde = hoy.AddDays(-29);
                hasta = hoy;
                break;

            case "mes":
                desde = new DateTime(hoy.Year, hoy.Month, 1);
                hasta = hoy;
                break;

            case "mesanterior":
                DateTime mesAnterior = hoy.AddMonths(-1);

                desde = new DateTime(
                    mesAnterior.Year,
                    mesAnterior.Month,
                    1);

                hasta = new DateTime(
                    mesAnterior.Year,
                    mesAnterior.Month,
                    DateTime.DaysInMonth(
                        mesAnterior.Year,
                        mesAnterior.Month));

                break;

            case "personalizado":

                desde = fechaDesde?.Date ?? hoy;
                hasta = fechaHasta?.Date ?? hoy;

                if (hasta < desde)
                {
                    (desde, hasta) = (hasta, desde);
                }

                break;

            default:

                periodo = "hoy";
                desde = hoy;
                hasta = hoy;

                break;
        }

        DateTime hastaExclusivo = hasta.AddDays(1);

        // =========================================================
        // INFORMACIÓN GENERAL
        // =========================================================

        ViewBag.TotalUsuarios = await _context.Usuarios
            .CountAsync(u => u.Activo);

        ViewBag.TotalMesas = await _context.Mesas
            .CountAsync();

        ViewBag.MesasDisponibles = await _context.Mesas
            .CountAsync(m => m.Estado == EstadoMesa.Disponible);

        ViewBag.MesasOcupadas = await _context.Mesas
            .CountAsync(m => m.Estado != EstadoMesa.Disponible);

        ViewBag.TotalPlatos = await _context.Platos
            .CountAsync();

        ViewBag.PlatosDisponibles = await _context.Platos
            .CountAsync(p => p.Estado == EstadoPlato.Disponible);

        ViewBag.TotalIngredientes = await _context.Ingredientes
            .CountAsync();

        ViewBag.AlertasInventario = await _context.Ingredientes
            .CountAsync(i =>
                i.CantidadDisponible <= i.CantidadMinima);

        ViewBag.EmpleadosActivos = await _context.Usuarios
            .CountAsync(u => u.Activo);

        ViewBag.TotalPedidos = await _context.Pedidos
            .CountAsync();

        ViewBag.PedidosPendientes = await _context.Pedidos
            .CountAsync(p =>
                p.Estado == EstadoPedido.Pendiente);

        ViewBag.PedidosEnPreparacion = await _context.Pedidos
            .CountAsync(p =>
                p.Estado == EstadoPedido.EnPreparacion);

        ViewBag.PedidosListos = await _context.Pedidos
            .CountAsync(p =>
                p.Estado == EstadoPedido.Listo);

        // =========================================================
        // PEDIDOS ENTREGADOS
        // =========================================================

        var pedidosPeriodo = await _context.Pedidos
            .Where(p =>
                p.Estado == EstadoPedido.Entregado &&
                p.Fecha >= desde &&
                p.Fecha < hastaExclusivo)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Plato)
            .AsNoTracking()
            .ToListAsync();

        // =========================================================
        // INGRESOS
        // =========================================================

        decimal ingresos = pedidosPeriodo
            .SelectMany(p => p.Detalles)
            .Sum(d => d.Cantidad * d.Plato.Precio);

        int pedidosEntregados = pedidosPeriodo.Count;

        int platosVendidos = pedidosPeriodo
            .SelectMany(p => p.Detalles)
            .Sum(d => d.Cantidad);

        decimal ticketPromedio = pedidosEntregados > 0
            ? ingresos / pedidosEntregados
            : 0;

        // =========================================================
        // PLATO MÁS VENDIDO
        // =========================================================

        var platoMasVendido = pedidosPeriodo
            .SelectMany(p => p.Detalles)
            .GroupBy(d => new
            {
                d.IdPlato,
                d.Plato.Nombre
            })
            .Select(g => new
            {
                Nombre = g.Key.Nombre,
                Cantidad = g.Sum(d => d.Cantidad),
                Ingresos = g.Sum(d =>
                    d.Cantidad * d.Plato.Precio)
            })
            .OrderByDescending(x => x.Cantidad)
            .FirstOrDefault();

        // =========================================================
        // TOP 5 PLATOS
        // =========================================================

        var topPlatos = pedidosPeriodo
            .SelectMany(p => p.Detalles)
            .GroupBy(d => new
            {
                d.IdPlato,
                d.Plato.Nombre
            })
            .Select(g => new
            {
                Nombre = g.Key.Nombre,
                Cantidad = g.Sum(d => d.Cantidad),
                Ingresos = g.Sum(d =>
                    d.Cantidad * d.Plato.Precio)
            })
            .OrderByDescending(x => x.Cantidad)
            .Take(5)
            .ToList();

        // =========================================================
        // INGRESOS POR DÍA
        // =========================================================

        var ingresosPorDia = pedidosPeriodo
            .SelectMany(p =>
                p.Detalles.Select(d => new
                {
                    Fecha = p.Fecha.Date,
                    Ingreso = d.Cantidad * d.Plato.Precio
                }))
            .GroupBy(x => x.Fecha)
            .Select(g => new
            {
                Fecha = g.Key,
                Ingresos = g.Sum(x => x.Ingreso)
            })
            .OrderBy(x => x.Fecha)
            .ToList();

        // =========================================================
        // PEDIDOS POR DÍA
        // =========================================================

        var pedidosPorDia = pedidosPeriodo
            .GroupBy(p => p.Fecha.Date)
            .Select(g => new
            {
                Fecha = g.Key,
                Cantidad = g.Count()
            })
            .OrderBy(x => x.Fecha)
            .ToList();

        // =========================================================
        // TIPO DE PEDIDO
        // =========================================================

        int pedidosMesa = pedidosPeriodo
            .Count(p => p.TipoPedido == TipoPedido.Mesa);

        int pedidosParaLlevar = pedidosPeriodo
            .Count(p => p.TipoPedido == TipoPedido.ParaLlevar);

        decimal ingresosMesa = pedidosPeriodo
            .Where(p => p.TipoPedido == TipoPedido.Mesa)
            .SelectMany(p => p.Detalles)
            .Sum(d => d.Cantidad * d.Plato.Precio);

        decimal ingresosParaLlevar = pedidosPeriodo
            .Where(p => p.TipoPedido == TipoPedido.ParaLlevar)
            .SelectMany(p => p.Detalles)
            .Sum(d => d.Cantidad * d.Plato.Precio);

        // =========================================================
        // DATOS PARA LA VISTA
        // =========================================================

        ViewBag.Periodo = periodo;

        ViewBag.FechaDesde = desde.ToString("yyyy-MM-dd");
        ViewBag.FechaHasta = hasta.ToString("yyyy-MM-dd");

        ViewBag.FechaDesdeTexto = desde.ToString("dd/MM/yyyy");
        ViewBag.FechaHastaTexto = hasta.ToString("dd/MM/yyyy");

        ViewBag.Ingresos = ingresos;
        ViewBag.PedidosEntregados = pedidosEntregados;
        ViewBag.PlatosVendidos = platosVendidos;
        ViewBag.TicketPromedio = ticketPromedio;

        ViewBag.PlatoMasVendido =
            platoMasVendido?.Nombre ?? "Sin ventas";

        ViewBag.CantidadPlatoMasVendido =
            platoMasVendido?.Cantidad ?? 0;

        ViewBag.IngresosPlatoMasVendido =
            platoMasVendido?.Ingresos ?? 0;

        ViewBag.TopPlatos = topPlatos;
        ViewBag.IngresosPorDia = ingresosPorDia;
        ViewBag.PedidosPorDia = pedidosPorDia;

        ViewBag.PedidosMesa = pedidosMesa;
        ViewBag.PedidosParaLlevar = pedidosParaLlevar;

        ViewBag.IngresosMesa = ingresosMesa;
        ViewBag.IngresosParaLlevar = ingresosParaLlevar;

        return View();
    }
}

}
