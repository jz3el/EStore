using EStore.Entity.DTO.Product;
using EStore.Services.Common.Behaviors;

namespace EStore.Services.Interfaces
{
    public interface IProductService
    {
        Task<Result<ProductResponseDto>> CreateAsync(ProductCreateDto dto);
        Task<Result<ProductResponseDto>> UpdateAsync(int id, ProductUpdateDto dto);

        Task<Result<ProductResponseDto?>> GetAsync(int id);
        Task<Result<IEnumerable<ProductResponseDto>>> GetAllAsync(int schoolId);

        Task<Result<bool>> DeleteAsync(int id);
    }
}
