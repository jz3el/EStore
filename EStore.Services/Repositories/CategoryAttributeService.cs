using EStore.Entity.DTO.CategoryAttribute;
using EStore.Entity.Models;
using EStore.Services.Common.Behaviors;
using EStore.Services.Common.Constant;
using EStore.Services.Interfaces;
using EStore.Services.Interfaces.GenericClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EStore.Services.Repositories
{
    public class CategoryAttributeService : ICategoryAttributeService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CategoryAttributeService> _logger;
        private readonly IGenericUserClientRepository _userClient;

        public CategoryAttributeService(
            ApplicationDbContext db,
            ILogger<CategoryAttributeService> logger,
            IGenericUserClientRepository userClient)
        {
            _db = db;
            _logger = logger;
            _userClient = userClient;
        }

        // ---------------- VALIDATE ADMIN ----------------
        private async Task<(bool Ok, string Message)> ValidateAdminAsync(Guid adminId, int schoolId)
        {
            try
            {
                var json = await _userClient.GetAsync<JsonElement>(
                    $"{ApiConstants.GetUserDetails}/{adminId}");

                if (!json.TryGetProperty("success", out var successElem) || !successElem.GetBoolean())
                    return (false, "Admin not found.");

                if (!json.TryGetProperty("data", out var dataElem))
                    return (false, "Invalid response.");

                int adminSchool = dataElem.GetProperty("schoolId").GetInt32();
                if (adminSchool != schoolId)
                    return (false, $"Admin belongs to {adminSchool}, not {schoolId}.");

                var roles = dataElem.GetProperty("roles")
                    .EnumerateArray()
                    .Select(r => r.GetString())
                    .ToList();

                var allowed = new[] { "super_admin", "store_admin" };

                if (!roles.Any(r => allowed.Contains(r, StringComparer.OrdinalIgnoreCase)))
                    return (false, "Admin does not have permission.");

                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin validation failed.");
                return (false, "Admin validation failed.");
            }
        }


        // ---------------- GET ALL ----------------
        public async Task<Result<IEnumerable<CategoryAttributeResponseDto>>> GetAllAsync(int schoolId)
        {
            var list = await _db.CategoryAttributes
                .Where(a => a.SchoolId == schoolId)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return new Result<IEnumerable<CategoryAttributeResponseDto>>
            {
                Success = true,
                StatusCode = "200",
                Data = list.Select(a => new CategoryAttributeResponseDto
                {
                    Id = a.Id,
                    AttributeName = a.Name,
                    Values = a.Values,
                    SchoolId = a.SchoolId,
                    AdminId = a.AdminId,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                })
            };
        }


        // ---------------- GET BY ID ----------------
        public async Task<Result<CategoryAttributeResponseDto?>> GetByIdAsync(int id)
        {
            var a = await _db.CategoryAttributes.FindAsync(id);
            if (a == null)
                return new Result<CategoryAttributeResponseDto?>
                {
                    Success = false,
                    StatusCode = "404",
                    Message = "Attribute not found"
                };

            return new Result<CategoryAttributeResponseDto?>
            {
                Success = true,
                StatusCode = "200",
                Data = new CategoryAttributeResponseDto
                {
                    Id = a.Id,
                    AttributeName = a.Name,
                    Values = a.Values,
                    SchoolId = a.SchoolId,
                    AdminId = a.AdminId,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                }
            };
        }


        // ---------------- CREATE ----------------
        public async Task<Result<CategoryAttributeResponseDto>> CreateAsync(CategoryAttributeCreateDto dto)
        {
            var check = await ValidateAdminAsync(dto.AdminId, dto.SchoolId);
            if (!check.Ok)
                return new Result<CategoryAttributeResponseDto>
                {
                    Success = false,
                    StatusCode = "403",
                    Message = check.Message
                };

            if (await _db.CategoryAttributes.AnyAsync(a =>
                    a.Name == dto.AttributeName && a.SchoolId == dto.SchoolId))
                return new Result<CategoryAttributeResponseDto>
                {
                    Success = false,
                    StatusCode = "409",
                    Message = "Attribute already exists."
                };

            var entity = new CategoryAttribute
            {
                Name = dto.AttributeName,
                Values = dto.Values,
                AdminId = dto.AdminId,
                SchoolId = dto.SchoolId,
            };

            _db.CategoryAttributes.Add(entity);
            await _db.SaveChangesAsync();

            return new Result<CategoryAttributeResponseDto>
            {
                Success = true,
                StatusCode = "201",
                Message = "Attribute created",
                Data = new CategoryAttributeResponseDto
                {
                    Id = entity.Id,
                    AttributeName = entity.Name,
                    Values = entity.Values,
                    AdminId = entity.AdminId,
                    SchoolId = entity.SchoolId,
                    IsActive = entity.IsActive,
                    CreatedAt = entity.CreatedAt
                }
            };
        }


        // ---------------- UPDATE ----------------
        public async Task<Result<CategoryAttributeResponseDto>> UpdateAsync(int id, CategoryAttributeUpdateDto dto)
        {
            var a = await _db.CategoryAttributes.FindAsync(id);
            if (a == null)
                return new Result<CategoryAttributeResponseDto>
                {
                    Success = false,
                    StatusCode = "404",
                    Message = "Attribute not found"
                };

            var check = await ValidateAdminAsync(a.AdminId, a.SchoolId);
            if (!check.Ok)
                return new Result<CategoryAttributeResponseDto>
                {
                    Success = false,
                    StatusCode = "403",
                    Message = check.Message
                };

            if (await _db.CategoryAttributes.AnyAsync(x =>
                    x.Name == dto.AttributeName &&
                    x.Id != id &&
                    x.SchoolId == a.SchoolId))
                return new Result<CategoryAttributeResponseDto>
                {
                    Success = false,
                    StatusCode = "409",
                    Message = "Attribute name already exists."
                };

            a.Name = dto.AttributeName;
            a.Values = dto.Values;
            a.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();

            return new Result<CategoryAttributeResponseDto>
            {
                Success = true,
                StatusCode = "200",
                Message = "Attribute updated",
                Data = new CategoryAttributeResponseDto
                {
                    Id = a.Id,
                    AttributeName = a.Name,
                    Values = a.Values,
                    AdminId = a.AdminId,
                    SchoolId = a.SchoolId,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                }
            };
        }


        // ---------------- DELETE ----------------
        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var a = await _db.CategoryAttributes.FindAsync(id);
            if (a == null)
                return new Result<bool>
                {
                    Success = false,
                    StatusCode = "404",
                    Message = "Attribute not found",
                    Data = false
                };

            var check = await ValidateAdminAsync(a.AdminId, a.SchoolId);
            if (!check.Ok)
                return new Result<bool>
                {
                    Success = false,
                    StatusCode = "403",
                    Message = check.Message,
                    Data = false
                };

            _db.CategoryAttributes.Remove(a);
            await _db.SaveChangesAsync();

            return new Result<bool>
            {
                Success = true,
                StatusCode = "200",
                Message = "Attribute deleted",
                Data = true
            };
        }
    }
}
