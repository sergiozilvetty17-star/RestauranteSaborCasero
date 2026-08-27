using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestauranteSaborCasero.Models
{
    public class PlatoIngrediente
    {
        public int IdPlato { get; set; }

        public int IdIngrediente { get; set; }

        [Required]
        [Range(0.01, 100000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CantidadNecesaria { get; set; }

        // Relaciones
        public Plato Plato { get; set; } = null!;

        public Ingrediente Ingrediente { get; set; } = null!;
    }
}