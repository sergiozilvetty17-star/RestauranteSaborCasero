using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestauranteSaborCasero.Models
{
    // 1. Agregamos las opciones de estado
    public enum EstadoCompra
    {
        Pendiente,
        Realizada,
        Cancelada
    }

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

        // 2. Agregamos la nueva columna de estado por defecto en 'Realizada' o 'Pendiente'
        [Required]
        public EstadoCompra Estado { get; set; } = EstadoCompra.Pendiente;

        // Relaciones
        public Usuario Usuario { get; set; } = null!;
        public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
    }
}