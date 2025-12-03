using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.Models
{
    [Table("PurchaseItems", Schema = "EStore")]
    public class PurchaseItem
    {
        [Key]
        public int Id { get; set; }

        // ------------ FK to Purchase ------

        [Required]
        public int PurchaseId { get; set; }

        public Purchase? Purchase { get; set; }

        // ------------ Product  ------------

        [Required]
        public int ProductId { get; set; }

        public int? VariantId { get; set; }  // size/color variant etc., optional for now

        // ------------ Quantities & Cost ----

        [Required]
        public int QuantityOrdered { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        // Running total of how much has been received across invoices
        public int QuantityReceived { get; set; } = 0;

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
}
