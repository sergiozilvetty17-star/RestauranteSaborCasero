using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestauranteSaborCasero.Models
{
    public class DetalleCompra
    {
        [Key]
        public int IdDetalleCompra { get; set; }

        [Required]
        public int IdCompra { get; set; }

        [Required]
        public int IdIngrediente { get; set; }

        [Required]
        [Range(0.01, 100000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Cantidad { get; set; }

        // Relaciones
        public Compra Compra { get; set; } = null!;

        public Ingrediente Ingrediente { get; set; } = null!;
    }
}