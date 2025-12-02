using EStore.Entity.DTO.Category;
using EStore.Services.Common.Behaviors;

namespace EStore.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<IEnumerable<CategoryResponseDto>>> GetAllAsync(int schoolId);
        Task<Result<CategoryResponseDto?>> GetByIdAsync(int id);
        Task<Result<CategoryResponseDto>> CreateAsync(CategoryCreateDto dto);
        Task<Result<CategoryResponseDto>> UpdateAsync(int id, CategoryUpdateDto dto);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
