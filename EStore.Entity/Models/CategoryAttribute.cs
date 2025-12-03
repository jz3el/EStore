using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Entity.Models
{
    [Table("CategoryAttributes", Schema = "EStore")]
    public class CategoryAttribute
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }

        [Required]
        public Guid AdminId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        // Will be stored as JSON (configure converter in DbContext)
        public List<string> Values { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
