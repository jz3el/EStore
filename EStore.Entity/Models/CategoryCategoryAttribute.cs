using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EStore.Entity.Models
{
    [Table("CategoryCategoryAttributes", Schema = "EStore")]
    public class CategoryCategoryAttribute
    {
        [Key] public int Id { get; set; }

        [Required] public int CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))] public Category? Category { get; set; }

        [Required] public int CategoryAttributeId { get; set; }
        [ForeignKey(nameof(CategoryAttributeId))] public CategoryAttribute? CategoryAttribute { get; set; }

        // Extra columns later (order, isRequiredOnProduct etc.) can be added here
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
