using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Models;

namespace RestauranteSaborCasero.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Plato> Platos { get; set; }
    }
}