using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EStore.Entity.Common.Enums;

namespace EStore.Entity.Models
{
    [Table("Categories", Schema = "EStore")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }

        [Required]
        public Guid AdminId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = default!;

        public bool HasSizeVariants { get; set; } = false;

        public SizeType SizeType { get; set; } = SizeType.None;

        [MaxLength(500)]
        public string? AvailableSizes { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
