using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.DTO.Product
{
    public class ProductCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int SchoolId { get; set; }

        [Required]
        public Guid AdminId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public bool HasVariants { get; set; }

        // simple product fields (no variants)
        public decimal? MRP { get; set; }
        public decimal? PurchasePrice { get; set; }
        public int? Quantity { get; set; }
        public int? ReorderLevel { get; set; }

        public string? Remarks { get; set; }

        // VARIANTS
        public List<ProductVariantCreateDto>? Variants { get; set; }

        // MULTIPLE IMAGES
        public List<IFormFile>? Images { get; set; }
    }
}
