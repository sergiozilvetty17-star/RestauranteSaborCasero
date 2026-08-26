using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Models; // Asegúrate de que esta ruta sea la correcta para tus modelos

namespace RestauranteSaborCasero.Data
{
    public class ApplicationDbContext : DbContext
    {
        // 1. Constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 2. Representación de todas tus tablas (DbSets)
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Plato> Platos { get; set; }
        public DbSet<Ingrediente> Ingredientes { get; set; }
        public DbSet<PlatoIngrediente> PlatoIngredientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallePedidos { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetalleCompras { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }

        // 3. Configuración especial (Llaves compuestas)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Es obligatorio configurar la llave primaria compuesta de la tabla intermedia
            modelBuilder.Entity<PlatoIngrediente>()
                .HasKey(pi => new { pi.IdPlato, pi.IdIngrediente });
        }
    }
}