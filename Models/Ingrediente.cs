using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestauranteSaborCasero.Models
{
    public enum EstadoIngrediente
    {
        Disponible,
        Agotado
    }

    public class Ingrediente
    {
        [Key]
        public int IdIngrediente { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Range(0, 100000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CantidadDisponible { get; set; }

        [Required]
        [Range(0, 100000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CantidadMinima { get; set; }

        [Required]
        [StringLength(30)]
        public string UnidadMedida { get; set; } = string.Empty;

        [Required]
        public EstadoIngrediente Estado { get; set; }
            = EstadoIngrediente.Disponible;

        // Relaciones
        public ICollection<PlatoIngrediente> PlatoIngredientes { get; set; }
            = new List<PlatoIngrediente>();

        public ICollection<DetalleCompra> DetallesCompra { get; set; }
            = new List<DetalleCompra>();

        public ICollection<Inventario> Inventarios { get; set; }
            = new List<Inventario>();
    }
}