using proyecto_final.Models;
using System.ComponentModel.DataAnnotations;

namespace RestauranteSaborCasero.Models
{
    public enum RolUsuario
    {
        Mesero,
        Cocinero,
        Administrador
    }

    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(255)]
        public string ContrasenaHash { get; set; } = string.Empty;

        [Required]
        public RolUsuario Rol { get; set; }

        public bool Activo { get; set; } = true;

        // Relaciones
        public ICollection<Pedido> Pedidos { get; set; }
            = new List<Pedido>();

        public ICollection<Compra> Compras { get; set; }
            = new List<Compra>();
    }
}