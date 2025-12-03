using System;

namespace EStore.Entity.DTO.Product
{
    public class ProductVariantResponseDto
    {
        public int Id { get; set; }

        public int AttributeId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public string AttributeValue { get; set; } = string.Empty;

        public decimal MRP { get; set; }
        public decimal PurchasePrice { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
    }
}
