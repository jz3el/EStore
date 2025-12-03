using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.DTO.Purchase
{
    public class PurchaseItemResponseDto
    {
        public int Id { get; set; }

        public int PurchaseId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty; // shown in PO list

        public int? VariantId { get; set; }

        public int QuantityOrdered { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotalCost { get; set; }

        public int QuantityReceived { get; set; }

        public string? Remarks { get; set; }
    }
}
