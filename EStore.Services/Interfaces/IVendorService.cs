using EStore.Entity.DTO.Vendor;
using EStore.Services.Common.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Services.Interfaces
{
    public interface IVendorService
    {
        Task<Result<IEnumerable<VendorResponseDto>>> GetAllAsync(int schoolId);
        Task<Result<VendorResponseDto?>> GetByIdAsync(int id);
        Task<Result<VendorResponseDto>> CreateAsync(VendorCreateDto dto);
        Task<Result<VendorResponseDto>> UpdateAsync(int id, VendorUpdateDto dto);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
