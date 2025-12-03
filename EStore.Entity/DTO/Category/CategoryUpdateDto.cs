using System.ComponentModel.DataAnnotations;

public class CategoryUpdateDto
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int SchoolId { get; set; }

    [Required]
    public Guid AdminId { get; set; }

    public bool IsActive { get; set; } = true;

    // new/updated list of attribute ids
    public List<int> AttributeIds { get; set; } = new();
}
