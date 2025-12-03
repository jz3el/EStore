using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.Models
{
    [Table("ProductVariants", Schema = "EStore")]
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [Required]
        public int AttributeId { get; set; }   // link to CategoryAttribute.Id

        [MaxLength(100)]
        public string AttributeValue { get; set; } = string.Empty;  // e.g. "S", "Blue", "Two Line"

        [Required]
        public decimal MRP { get; set; }

        [Required]
        public decimal PurchasePrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int ReorderLevel { get; set; }
    }
}
