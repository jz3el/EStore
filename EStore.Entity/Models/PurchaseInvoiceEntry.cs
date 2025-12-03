using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.Models
{
    [Table("PurchaseInvoiceEntries", Schema = "EStore")]
    public class PurchaseInvoiceEntry
    {
        [Key]
        public int Id { get; set; }

        // ------------ PO + Invoice Info --------

        [Required]
        public int PurchaseOrderId { get; set; }   // FK → Purchase.Id

        [Required, MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal InvoiceAmount { get; set; }

        public DateTime InvoiceDate { get; set; }

        public DateTime? DueDate { get; set; }

        // ------------ Delivery Info ------------

        [Required]
        public int PurchaseItemId { get; set; }    // FK → PurchaseItem.Id

        public DateTime DeliveryDate { get; set; }

        [Required]
        public int QuantityReceived { get; set; }

        [MaxLength(500)]
        public string? DeliveryNotes { get; set; }

        // ------------ Meta ---------------------

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ------------ Navigation ---------------

        public Purchase? Purchase { get; set; }

        public PurchaseItem? PurchaseItem { get; set; }
    }
}
