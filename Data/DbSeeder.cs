using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Models;

namespace RestauranteSaborCasero.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            var existeAdmin = await context.Usuarios
                .AnyAsync(u => u.Rol == RolUsuario.Administrador);

            if (!existeAdmin)
            {
                var usuario = new Usuario
                {
                    Nombre = "Administrador",
                    Correo = "admin@saborcasero.com",
                    ContrasenaHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Rol = RolUsuario.Administrador,
                    Activo = true
                };

                context.Usuarios.Add(usuario);

                await context.SaveChangesAsync();
            }
        }
    }
}