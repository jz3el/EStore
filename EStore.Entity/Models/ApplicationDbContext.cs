using Microsoft.EntityFrameworkCore;

namespace EStore.Entity.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ----------------- DB SETS --------------------
        public DbSet<Category> Categories { get; set; }

        // If you add more modules later (Vendors, Products, Purchases etc)
        // add DbSets here.
        // public DbSet<Product> Products { get; set; }
        // public DbSet<Vendor> Vendors { get; set; }
        // public DbSet<Purchase> Purchases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure schema EStore exists
            modelBuilder.HasDefaultSchema("EStore");

            // Category table mapping (optional, already mapped by attributes)
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories", "EStore");
            });
        }
    }
}
