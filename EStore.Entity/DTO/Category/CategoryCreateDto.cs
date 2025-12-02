using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using EStore.Entity.Common.Enums;

namespace EStore.Entity.DTO.Category
{
    public class CategoryCreateDto
    {
        [Required]
        public int SchoolId { get; set; }

        [Required]
        public Guid AdminId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public bool HasSizeVariants { get; set; } = false;

        public SizeType SizeType { get; set; } = SizeType.None;

        public string? AvailableSizes { get; set; }

        public IFormFile? Image { get; set; }  // Optional
    }
}
