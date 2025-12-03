using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EStore.Entity.Models
{
    [Table("Categories", Schema = "EStore")]
    public class Category
    {
        [Key] public int Id { get; set; }

        [Required] public int SchoolId { get; set; }

        [Required] public Guid AdminId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // navigation: many-to-many via join table
        public ICollection<CategoryCategoryAttribute> CategoryAttributes { get; set; } = new List<CategoryCategoryAttribute>();
    }
}
