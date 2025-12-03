using System.ComponentModel.DataAnnotations;

namespace EStore.Entity.DTO.CategoryAttribute
{
    public class CategoryAttributeCreateDto
    {
        [Required, MaxLength(150)]
        public string AttributeName { get; set; } = string.Empty;

        [Required]
        public List<string> Values { get; set; } = new();

        [Required]
        public int SchoolId { get; set; }

        [Required]
        public Guid AdminId { get; set; }
    }
}
