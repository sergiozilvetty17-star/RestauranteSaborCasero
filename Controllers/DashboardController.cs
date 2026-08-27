using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Data;

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

        // ==========================================
        // GET: Dashboard
        // ==========================================

        public async Task<IActionResult> Index()
        {
            // Cantidad de usuarios activos
            ViewBag.TotalUsuarios = await _context.Usuarios
                .CountAsync(u => u.Activo);

            // Cantidad de mesas
            ViewBag.TotalMesas = await _context.Mesas
                .CountAsync();

            // Cantidad de mesas disponibles
            ViewBag.MesasDisponibles = await _context.Mesas
                .CountAsync(m => m.Estado == Models.EstadoMesa.Disponible);

            // Cantidad de platos
            ViewBag.TotalPlatos = await _context.Platos
                .CountAsync();

            // Cantidad de ingredientes
            ViewBag.TotalIngredientes = await _context.Ingredientes
                .CountAsync();

            // Cantidad de pedidos
            ViewBag.TotalPedidos = await _context.Pedidos
                .CountAsync();

            // Pedidos pendientes
            ViewBag.PedidosPendientes = await _context.Pedidos
                .CountAsync(p =>
                    p.Estado == Models.EstadoPedido.Pendiente);

            // Pedidos en preparación
            ViewBag.PedidosEnPreparacion = await _context.Pedidos
                .CountAsync(p =>
                    p.Estado == Models.EstadoPedido.EnPreparacion);

            return View();
        }
    }
}