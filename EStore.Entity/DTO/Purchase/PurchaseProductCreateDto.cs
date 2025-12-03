using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.DTO.Purchase
{
    // One row from the "Products" section in the Create Purchase Order popup
    public class PurchaseProductCreateDto
    {
        [Required, MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;   // for display/logging

        [Required]
        public int ProductId { get; set; }                        // actual FK to Product

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }                         // Quantity *

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PurchasePricePerUnit { get; set; }         // Purchase Price per Unit *

        [MaxLength(500)]
        public string? Remarks { get; set; }                      // optional remarks
    }

    // Main DTO for creating a purchase order
    public class PurchaseCreateDto
    {
        [Required]
        public int SchoolId { get; set; }

        [Required]
        public Guid AdminId { get; set; }

        [Required]
        public int VendorId { get; set; }

        // List of products in this purchase order
        [Required]
        [MinLength(1, ErrorMessage = "At least one product is required.")]
        public List<PurchaseProductCreateDto> Products { get; set; } = new();
    }
}
