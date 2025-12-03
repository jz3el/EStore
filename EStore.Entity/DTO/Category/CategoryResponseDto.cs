using EStore.Entity.DTO.CategoryAttribute;
public class CategoryResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SchoolId { get; set; }
    public Guid AdminId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // full attribute objects for front-end list display
    public List<CategoryAttributeResponseDto> Attributes { get; set; } = new();
}
