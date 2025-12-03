using Microsoft.EntityFrameworkCore;

namespace EStore.Entity.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<PurchaseInvoiceEntry> PurchaseInvoiceEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("EStore");

            // Vendor
            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.ToTable("Vendors", "EStore");
            });

            // Purchase
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.ToTable("Purchases", "EStore");

                entity.HasMany(p => p.Items)
                      .WithOne(i => i.Purchase!)
                      .HasForeignKey(i => i.PurchaseId)
                      .OnDelete(DeleteBehavior.Cascade); // OK
            });

            // PurchaseItem
            modelBuilder.Entity<PurchaseItem>(entity =>
            {
                entity.ToTable("PurchaseItems", "EStore");
            });

            // PurchaseInvoiceEntry
            modelBuilder.Entity<PurchaseInvoiceEntry>(entity =>
            {
                entity.ToTable("PurchaseInvoiceEntries", "EStore");

                // ❌ DO NOT cascade from Purchase to InvoiceEntries
                entity.HasOne(e => e.Purchase)
                      .WithMany()
                      .HasForeignKey(e => e.PurchaseOrderId)
                      .OnDelete(DeleteBehavior.NoAction);

                // ✅ Cascade from PurchaseItem to InvoiceEntries is ok
                entity.HasOne(e => e.PurchaseItem)
                      .WithMany()
                      .HasForeignKey(e => e.PurchaseItemId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
