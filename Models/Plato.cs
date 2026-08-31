using RestauranteSaborCasero.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestauranteSaborCasero.Models
{
    public enum EstadoPlato
    {
        Disponible,
        Agotado
    }

    public class Plato
    {
        [Key]
        public int IdPlato { get; set; }

        [Required(ErrorMessage = "El nombre del plato es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        [Required]
        public EstadoPlato Estado { get; set; }
            = EstadoPlato.Disponible;

        [Required]
        [Range(1, 1000)]
        public int TiempoEstimado { get; set; }

        // Relaciones
        public ICollection<PlatoIngrediente> PlatoIngredientes { get; set; }
            = new List<PlatoIngrediente>();

        public ICollection<DetallePedido> DetallesPedido { get; set; }
            = new List<DetallePedido>();
    }
}