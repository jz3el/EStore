using System.ComponentModel.DataAnnotations;

namespace EStore.Entity.DTO.CategoryAttribute
{
    public class CategoryAttributeUpdateDto
    {
        [Required, MaxLength(150)]
        public string AttributeName { get; set; } = string.Empty;

        [Required]
        public List<string> Values { get; set; } = new();

        public bool IsActive { get; set; } = true;
    }
}
