using EStore.Entity.DTO.CategoryAttribute;
using EStore.Services.Common.Behaviors;

namespace EStore.Services.Interfaces
{
    public interface ICategoryAttributeService
    {
        Task<Result<IEnumerable<CategoryAttributeResponseDto>>> GetAllAsync(int schoolId);
        Task<Result<CategoryAttributeResponseDto?>> GetByIdAsync(int id);
        Task<Result<CategoryAttributeResponseDto>> CreateAsync(CategoryAttributeCreateDto dto);
        Task<Result<CategoryAttributeResponseDto>> UpdateAsync(int id, CategoryAttributeUpdateDto dto);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
