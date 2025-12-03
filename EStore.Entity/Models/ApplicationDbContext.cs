using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using System.Collections.Generic;

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
        // ----------------- DB SETS --------------------
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryAttribute> CategoryAttributes { get; set; }
        public DbSet<CategoryCategoryAttribute> CategoryCategoryAttributes { get; set; }

        // PRODUCT TABLES
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

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
            // ---------------- Category ----------------
            modelBuilder.Entity<Category>(b =>
            {
                b.ToTable("Categories", "EStore");
                b.HasIndex(c => new { c.SchoolId, c.Name }).IsUnique(false);
            });

            // ---------------- CategoryAttribute ----------------
            modelBuilder.Entity<CategoryAttribute>(b =>
            {
                b.ToTable("CategoryAttributes", "EStore");
            });

            // Many-to-many table
            modelBuilder.Entity<CategoryCategoryAttribute>(b =>
            {
                b.ToTable("CategoryCategoryAttributes", "EStore");

                b.HasIndex(x => new { x.CategoryId, x.CategoryAttributeId })
                 .IsUnique()
                 .HasDatabaseName("UX_Category_Attribute");

                b.HasOne(x => x.Category)
                 .WithMany(c => c.CategoryAttributes)
                 .HasForeignKey(x => x.CategoryId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.CategoryAttribute)
                 .WithMany()
                 .HasForeignKey(x => x.CategoryAttributeId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // JSON converter for attribute values
            var listToJsonConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v ?? new List<string>(), (JsonSerializerOptions?)null),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            );

            modelBuilder.Entity<CategoryAttribute>()
                .Property(a => a.Values)
                .HasConversion(listToJsonConverter)
                .HasColumnType("nvarchar(max)");

            // ---------------- Product ----------------
            modelBuilder.Entity<Product>(b =>
            {
                b.ToTable("Products", "EStore");

                b.HasOne(p => p.Category)
                 .WithMany()
                 .HasForeignKey(p => p.CategoryId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- ProductVariant ----------------
            modelBuilder.Entity<ProductVariant>(b =>
            {
                b.ToTable("ProductVariants", "EStore");

                b.HasOne(v => v.Product)
                 .WithMany(p => p.Variants)
                 .HasForeignKey(v => v.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------- ProductImage ----------------
            modelBuilder.Entity<ProductImage>(b =>
            {
                b.ToTable("ProductImages", "EStore");

                b.HasOne(i => i.Product)
                 .WithMany(p => p.Images)
                 .HasForeignKey(i => i.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

        }

    }
}
