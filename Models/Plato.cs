using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestauranteSaborCasero.Models
{
    public class Plato
    {
        [Key]
        public int IdPlato { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Range(0.01, 100000)]
        [Column(TypeName = "decimal(10.2)")]
        public decimal Precio { get; set; }

        [Required]
        public string Estado { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int TiempoEstimado { get; set; }
    }
}