using proyecto_final.Models;
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

        [Required]
        public TimeSpan HoraInicio { get; set; }

        public TimeSpan? HoraFin { get; set; }

        [Required]
        public EstadoPedido Estado { get; set; }
            = EstadoPedido.Pendiente;

        // Relaciones
        public Usuario Usuario { get; set; } = null!;

        public Mesa? Mesa { get; set; }

        public ICollection<DetallePedido> Detalles { get; set; }
            = new List<DetallePedido>();
    }
}