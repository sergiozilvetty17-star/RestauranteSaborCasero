using System.ComponentModel.DataAnnotations;

namespace RestauranteSaborCasero.Models
{
    public class DetallePedido
    {
        [Key]
        public int IdDetalle { get; set; }

        [Required]
        public int IdPedido { get; set; }

        [Required]
        public int IdPlato { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Cantidad { get; set; }

        [StringLength(255)]
        public string? IndicacionesExtra { get; set; }

        // Relaciones
        public Pedido Pedido { get; set; } = null!;

        public Plato Plato { get; set; } = null!;
    }
}