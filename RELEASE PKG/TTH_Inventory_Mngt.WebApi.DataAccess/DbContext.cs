using Microsoft.EntityFrameworkCore;
using TTH_Inventory_Mngt.WebApi.Common.Models;

namespace TTH_Inventory_Mngt.WebApi.DataAccess
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Products> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite primary key
            modelBuilder.Entity<Products>()
                .HasKey(p => new { p.InstitutionCode, p.ProductId });
        }
    }
}
