using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.DTO.Vendor
{
    public class VendorUpdateDto
    {
        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string GstNumber { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string ContactPerson { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Designation { get; set; }

        [Required, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? AccountNumber { get; set; }

        [MaxLength(20)]
        public string? IfscCode { get; set; }

        [Required, MaxLength(500)]
        public string ProductsSupplied { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
