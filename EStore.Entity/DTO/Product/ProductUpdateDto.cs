using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EStore.Entity.DTO.Product
{
    public class ProductUpdateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public bool HasVariants { get; set; }

        public decimal? MRP { get; set; }
        public decimal? PurchasePrice { get; set; }
        public int? Quantity { get; set; }
        public int? ReorderLevel { get; set; }

        public string? Remarks { get; set; }

        public List<ProductVariantUpdateDto>? Variants { get; set; }
        public List<IFormFile>? NewImages { get; set; }
        public List<int>? RemoveImageIds { get; set; }

        public int SchoolId { get; set; }
        public Guid AdminId { get; set; }
    }
}
