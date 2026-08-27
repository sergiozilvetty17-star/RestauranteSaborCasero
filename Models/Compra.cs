using RestauranteSaborCasero.Models;
using System.ComponentModel.DataAnnotations;

namespace RestauranteSaborCasero.Models
{
    public class Compra
    {
        [Key]
        public int IdCompra { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(150)]
        public string Proveedor { get; set; } = string.Empty;

        // Relaciones
        public Usuario Usuario { get; set; } = null!;

        public ICollection<DetalleCompra> Detalles { get; set; }
            = new List<DetalleCompra>();
    }
}