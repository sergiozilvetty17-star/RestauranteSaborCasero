using RestauranteSaborCasero.Models;
using System.ComponentModel.DataAnnotations;

namespace RestauranteSaborCasero.Models
{
    public enum EstadoMesa
    {
        Disponible,
        Ocupada,
        Mantenimiento
    }

    public class Mesa
    {
        [Key]
        public int IdMesa { get; set; }

        [Required(ErrorMessage = "El número de mesa es obligatorio.")]
        [Range(1, 999, ErrorMessage = "El número de mesa debe ser mayor a 0.")]
        public int NumeroMesa { get; set; }

        [Required]
        public EstadoMesa Estado { get; set; }
            = EstadoMesa.Disponible;

        // Relaciones
        public ICollection<Pedido> Pedidos { get; set; }
            = new List<Pedido>();
    }
}