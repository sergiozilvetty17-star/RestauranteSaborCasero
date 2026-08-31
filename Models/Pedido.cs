using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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

        [Required]
        public TimeSpan HoraInicio { get; set; }

        public TimeSpan? HoraFin { get; set; }

        public TimeSpan? HoraEnPreparacion { get; set; }

        public TimeSpan? HoraListo { get; set; }

        public TimeSpan? HoraEntregado { get; set; }

        public TimeSpan? HoraCancelado { get; set; }

        // ==========================================
        // ESTADO
        // ==========================================

        [Required]
        public EstadoPedido Estado { get; set; }
            = EstadoPedido.Pendiente;

        // ==========================================
        // RELACIONES
        // ==========================================

        [ValidateNever]
        public Usuario Usuario { get; set; } = null!;

        [ValidateNever]
        public Mesa? Mesa { get; set; }

        [ValidateNever]
        public ICollection<DetallePedido> Detalles { get; set; }
            = new List<DetallePedido>();
    }
}