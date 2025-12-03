using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EStore.Entity.Models
{
    [Table("Vendors", Schema = "EStore")]
    public class Vendor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }

        [Required]
        public Guid AdminId { get; set; }

        [MaxLength(20)]
        public string? VendorCode { get; set; }


        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = default!;

        [Required, MaxLength(50)]
        public string GstNumber { get; set; } = default!;

        [Required, MaxLength(150)]
        public string ContactPerson { get; set; } = default!;

        [MaxLength(100)]
        public string? Designation { get; set; }

        [Required, MaxLength(20)]
        public string Phone { get; set; } = default!;

        [Required, MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required, MaxLength(500)]
        public string Address { get; set; } = default!;

        [MaxLength(150)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? AccountNumber { get; set; }

        [MaxLength(20)]
        public string? IfscCode { get; set; }

        [Required, MaxLength(500)]
        public string ProductsSupplied { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
