namespace EStore.Entity.DTO.CategoryAttribute
{
    public class CategoryAttributeResponseDto
    {
        public int Id { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new();
        public int SchoolId { get; set; }
        public Guid AdminId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
