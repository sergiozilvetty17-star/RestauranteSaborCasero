using RestauranteSaborCasero.Models;
using System.ComponentModel.DataAnnotations;

namespace RestauranteSaborCasero.Models
{
    public enum TipoPedido
    {
        Mesa,
        ParaLlevar
    }

    public enum EstadoPedido
    {
        Pendiente,
        EnPreparacion,
        Listo,
        Entregado,
        Cancelado
    }

    public class Pedido
    {
        [Key]
        public int IdPedido { get; set; }

        [Required]
        public int IdMesero { get; set; }

        public int? IdMesa { get; set; }

        [Required]
        public TipoPedido TipoPedido { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        // ==========================================
        // HORAS DEL PEDIDO
        // ==========================================

        // Hora en que se creó el pedido
        [Required]
        public TimeSpan HoraInicio { get; set; }

        // Hora en que terminó el pedido
        public TimeSpan? HoraFin { get; set; }

        // Hora en que pasó a EnPreparacion
        public TimeSpan? HoraEnPreparacion { get; set; }

        // Hora en que pasó a Listo
        public TimeSpan? HoraListo { get; set; }

        // Hora en que pasó a Entregado
        public TimeSpan? HoraEntregado { get; set; }

        // Hora en que fue Cancelado
        public TimeSpan? HoraCancelado { get; set; }

        // ==========================================
        // ESTADO DEL PEDIDO
        // ==========================================

        [Required]
        public EstadoPedido Estado { get; set; }
            = EstadoPedido.Pendiente;

        // ==========================================
        // RELACIONES
        // ==========================================

        public Usuario Usuario { get; set; } = null!;

        public Mesa? Mesa { get; set; }

        public ICollection<DetallePedido> Detalles { get; set; }
            = new List<DetallePedido>();
    }
}