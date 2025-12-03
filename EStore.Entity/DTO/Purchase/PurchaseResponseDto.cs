using EStore.Entity.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.DTO.Purchase
{
    public class PurchaseResponseDto
    {
        public int Id { get; set; }

        public int SchoolId { get; set; }

        public Guid AdminId { get; set; }

        public int VendorId { get; set; }

        public string PurchaseOrderNumber { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }

        public PurchaseStatus Status { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Remarks { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public IEnumerable<PurchaseItemResponseDto> Items { get; set; }
            = new List<PurchaseItemResponseDto>();
    }
}
