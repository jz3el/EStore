using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.DTO.Product
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public bool HasVariants { get; set; }
        public string Status { get; set; }
        public string? Remarks { get; set; }

        public decimal? MRP { get; set; }
        public decimal? PurchasePrice { get; set; }
        public int? Quantity { get; set; }
        public int? ReorderLevel { get; set; }

        public List<ProductVariantResponseDto> Variants { get; set; } = new();
        public List<string> Images { get; set; } = new();
    }
}
