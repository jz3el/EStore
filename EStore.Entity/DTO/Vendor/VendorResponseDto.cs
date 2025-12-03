using System;

namespace EStore.Entity.DTO.Vendor
{
    public class VendorResponseDto
    {
        public int Id { get; set; }

        public int SchoolId { get; set; }
        public Guid AdminId { get; set; }

        // 🔹 NEW
        public string VendorCode { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;
        public string GstNumber { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? IfscCode { get; set; }

        public string ProductsSupplied { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
