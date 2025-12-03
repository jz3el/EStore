using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.DTO.Product
{
    public class ProductVariantCreateDto
    {
        public int AttributeId { get; set; }
        public string AttributeValue { get; set; } = string.Empty;

        public decimal MRP { get; set; }
        public decimal PurchasePrice { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
    }

}
