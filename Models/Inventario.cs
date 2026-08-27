using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestauranteSaborCasero.Models
{
    public enum TipoMovimientoInventario
    {
        Entrada,
        Salida,
        Ajuste
    }

    public class Inventario
    {
        [Key]
        public int IdInventario { get; set; }

        [Required]
        public int IdIngrediente { get; set; }

        [Required]
        public TipoMovimientoInventario TipoMovimiento { get; set; }

        [Required]
        [Range(0.01, 100000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Cantidad { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? Motivo { get; set; }

        // Relación
        public Ingrediente Ingrediente { get; set; } = null!;
    }
}