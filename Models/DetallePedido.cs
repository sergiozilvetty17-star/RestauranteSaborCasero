using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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

        // ==========================================
        // RELACIONES
        // ==========================================

        [ValidateNever]
        public Pedido Pedido { get; set; } = null!;

        [ValidateNever]
        public Plato Plato { get; set; } = null!;
    }
}