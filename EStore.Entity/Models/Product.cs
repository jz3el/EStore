using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.Models
{
    [Table("Products", Schema = "EStore")]
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }

        [Required]
        public Guid AdminId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ProductCode { get; set; } = string.Empty;   // Ex: PR001

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        public bool HasVariants { get; set; }

        // Simple product price (if HasVariants = false)
        public decimal? MRP { get; set; }
        public decimal? PurchasePrice { get; set; }
        public int? Quantity { get; set; }
        public int? ReorderLevel { get; set; }

        public string? Remarks { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // VARIANTS (Many)
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        // IMAGES (Many)
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
