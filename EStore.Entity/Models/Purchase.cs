using EStore.Entity.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.Models
{
    [Table("Purchases", Schema = "EStore")]
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        // ------------ Context ------------

        [Required]
        public int SchoolId { get; set; }

        [Required]
        public Guid AdminId { get; set; }

        // ------------ Vendor -------------

        [Required]
        public int VendorId { get; set; }   // FK → Vendors.Id

        // ------------ PO Info ------------

        [Required, MaxLength(20)]
        public string PurchaseOrderNumber { get; set; } = string.Empty; // e.g. PO001

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [Required]
        public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0m;

        [MaxLength(500)]
        public string? Remarks { get; set; }

        // ------------ Meta ---------------

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ------------ Navigation ---------

        public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    }
}
