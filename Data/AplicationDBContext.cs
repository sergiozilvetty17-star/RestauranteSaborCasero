using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Models;

namespace RestauranteSaborCasero.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ==========================================
        // TABLAS
        // ==========================================

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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ==========================================
            // USUARIO
            // ==========================================

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();


            // ==========================================
            // MESA
            // ==========================================

            modelBuilder.Entity<Mesa>()
                .HasIndex(m => m.NumeroMesa)
                .IsUnique();


            // ==========================================
            // PLATO - INGREDIENTE
            // ==========================================

            modelBuilder.Entity<PlatoIngrediente>()
                .HasKey(pi => new
                {
                    pi.IdPlato,
                    pi.IdIngrediente
                });

            modelBuilder.Entity<PlatoIngrediente>()
                .HasOne(pi => pi.Plato)
                .WithMany(p => p.PlatoIngredientes)
                .HasForeignKey(pi => pi.IdPlato)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlatoIngrediente>()
                .HasOne(pi => pi.Ingrediente)
                .WithMany(i => i.PlatoIngredientes)
                .HasForeignKey(pi => pi.IdIngrediente)
                .OnDelete(DeleteBehavior.Cascade);


            // ==========================================
            // PEDIDO - USUARIO
            // ==========================================

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(p => p.IdMesero)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // PEDIDO - MESA
            // ==========================================

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Mesa)
                .WithMany(m => m.Pedidos)
                .HasForeignKey(p => p.IdMesa)
                .OnDelete(DeleteBehavior.SetNull);


            // ==========================================
            // DETALLE PEDIDO - PEDIDO
            // ==========================================

            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(dp => dp.IdPedido)
                .OnDelete(DeleteBehavior.Cascade);


            // ==========================================
            // DETALLE PEDIDO - PLATO
            // ==========================================

            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.Plato)
                .WithMany(p => p.DetallesPedido)
                .HasForeignKey(dp => dp.IdPlato)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // COMPRA - USUARIO
            // ==========================================

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Compras)
                .HasForeignKey(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // DETALLE COMPRA - COMPRA
            // ==========================================

            modelBuilder.Entity<DetalleCompra>()
                .HasOne(dc => dc.Compra)
                .WithMany(c => c.Detalles)
                .HasForeignKey(dc => dc.IdCompra)
                .OnDelete(DeleteBehavior.Cascade);


            // ==========================================
            // DETALLE COMPRA - INGREDIENTE
            // ==========================================

            modelBuilder.Entity<DetalleCompra>()
                .HasOne(dc => dc.Ingrediente)
                .WithMany(i => i.DetallesCompra)
                .HasForeignKey(dc => dc.IdIngrediente)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // INVENTARIO - INGREDIENTE
            // ==========================================

            modelBuilder.Entity<Inventario>()
                .HasOne(inv => inv.Ingrediente)
                .WithMany(i => i.Inventarios)
                .HasForeignKey(inv => inv.IdIngrediente)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}