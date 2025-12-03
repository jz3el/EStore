namespace EStore.Entity.DTO.Product
{
    public class ProductVariantUpdateDto
    {
        public int? Id { get; set; } // null means new variant

        public int AttributeId { get; set; }
        public string AttributeValue { get; set; } = string.Empty;

        public decimal MRP { get; set; }
        public decimal PurchasePrice { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
    }
}
