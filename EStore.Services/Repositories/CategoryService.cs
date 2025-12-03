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
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CategoryService> _logger;
        private readonly IGenericUserClientRepository _userClient;

        public CategoryService(ApplicationDbContext db,
            ILogger<CategoryService> logger,
            IGenericUserClientRepository userClient)
        {
            _db = db;
            _logger = logger;
            _userClient = userClient;
        }

        // (re-use your existing ValidateAdminAsync implementation that calls SMS and checks roles & school)
        private async Task<(bool Ok, string Message)> ValidateAdminAsync(Guid adminId, int schoolId)
        {
            if (adminId == Guid.Empty) return (false, "adminId required");
            try
            {
                var json = await _userClient.GetAsync<JsonElement>($"{ApiConstants.GetUserDetails}/{adminId}");
                if (!json.TryGetProperty("success", out var successElem) || !successElem.GetBoolean())
                    return (false, "Admin not found");
                if (!json.TryGetProperty("data", out var dataElem)) return (false, "Invalid SMS response");
                if (!dataElem.TryGetProperty("schoolId", out var schElem)) return (false, "SchoolId missing");
                var adminSchool = schElem.GetInt32();
                if (adminSchool != schoolId) return (false, $"Admin belongs to school {adminSchool}");
                if (!dataElem.TryGetProperty("roles", out var rolesElem) || rolesElem.ValueKind != JsonValueKind.Array)
                    return (false, "Roles missing");
                var roles = rolesElem.EnumerateArray().Select(r => r.GetString()).ToList();
                var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "super_admin", "store_admin" };
                if (!roles.Any(r => allowed.Contains(r))) return (false, "Admin lacks permission");
                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateAdmin failed");
                return (false, "Failed to validate admin");
            }
        }

        public async Task<Result<IEnumerable<CategoryResponseDto>>> GetAllAsync(int schoolId)
        {
            var cats = await _db.Categories
                .AsNoTracking()
                .Where(c => c.SchoolId == schoolId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // fetch attribute ids then the attribute data in bulk for efficiency
            var categoryIds = cats.Select(c => c.Id).ToList();

            var maps = await _db.CategoryCategoryAttributes
                .AsNoTracking()
                .Where(m => categoryIds.Contains(m.CategoryId))
                .ToListAsync();

            var attributeIds = maps.Select(m => m.CategoryAttributeId).Distinct().ToList();

            var attributes = await _db.CategoryAttributes
                .AsNoTracking()
                .Where(a => attributeIds.Contains(a.Id))
                .ToListAsync();

            var attributeById = attributes.ToDictionary(a => a.Id);

            var result = cats.Select(c =>
            {
                var assigned = maps.Where(m => m.CategoryId == c.Id)
                                   .Select(m => m.CategoryAttributeId)
                                   .Where(id => attributeById.ContainsKey(id))
                                   .Select(id => new CategoryAttributeResponseDto
                                   {
                                       Id = attributeById[id].Id,
                                       AttributeName = attributeById[id].Name,
                                       Values = attributeById[id].Values,
                                       SchoolId = attributeById[id].SchoolId,
                                       AdminId = attributeById[id].AdminId,
                                       IsActive = attributeById[id].IsActive,
                                       CreatedAt = attributeById[id].CreatedAt
                                   }).ToList();

                return new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    SchoolId = c.SchoolId,
                    AdminId = c.AdminId,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    Attributes = assigned
                };
            });

            return new Result<IEnumerable<CategoryResponseDto>> { Success = true, StatusCode = "200", Data = result, Message = "Categories fetched" };
        }

        public async Task<Result<CategoryResponseDto?>> GetByIdAsync(int id)
        {
            var c = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return new Result<CategoryResponseDto?> { Success = false, StatusCode = "404", Message = "Category not found" };

            var maps = await _db.CategoryCategoryAttributes.AsNoTracking().Where(m => m.CategoryId == id).ToListAsync();
            var attrIds = maps.Select(m => m.CategoryAttributeId).ToList();
            var attributes = await _db.CategoryAttributes.AsNoTracking().Where(a => attrIds.Contains(a.Id)).ToListAsync();

            var resp = new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                SchoolId = c.SchoolId,
                AdminId = c.AdminId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                Attributes = attributes.Select(a => new CategoryAttributeResponseDto
                {
                    Id = a.Id,
                    AttributeName = a.Name,
                    Values = a.Values,
                    SchoolId = a.SchoolId,
                    AdminId = a.AdminId,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                }).ToList()
            };

            return new Result<CategoryResponseDto?> { Success = true, StatusCode = "200", Data = resp, Message = "Category fetched" };
        }

        public async Task<Result<CategoryResponseDto>> CreateAsync(CategoryCreateDto dto)
        {
            // validate
            var adminCheck = await ValidateAdminAsync(dto.AdminId, dto.SchoolId);
            if (!adminCheck.Ok) return new Result<CategoryResponseDto> { Success = false, StatusCode = "403", Message = adminCheck.Message };

            // duplicate name
            if (await _db.Categories.AnyAsync(c => c.Name == dto.Name && c.SchoolId == dto.SchoolId))
                return new Result<CategoryResponseDto> { Success = false, StatusCode = "409", Message = "Category already exists" };

            var category = new Category
            {
                SchoolId = dto.SchoolId,
                AdminId = dto.AdminId,
                Name = dto.Name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Categories.Add(category);
                await _db.SaveChangesAsync();

                // add mapping rows
                foreach (var attrId in dto.AttributeIds.Distinct())
                {
                    // optionally ensure attribute exists & belongs to same school
                    var attrExists = await _db.CategoryAttributes.AnyAsync(a => a.Id == attrId && a.SchoolId == dto.SchoolId);
                    if (!attrExists)
                    {
                        await tx.RollbackAsync();
                        return new Result<CategoryResponseDto> { Success = false, StatusCode = "422", Message = $"Attribute {attrId} not found for school." };
                    }

                    _db.CategoryCategoryAttributes.Add(new CategoryCategoryAttribute
                    {
                        CategoryId = category.Id,
                        CategoryAttributeId = attrId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Create category failed");
                return new Result<CategoryResponseDto> { Success = false, StatusCode = "500", Message = "Failed to create category" };
            }

            return await GetByIdAsync(category.Id) as Result<CategoryResponseDto>; // reuse mapping
        }

        public async Task<Result<CategoryResponseDto>> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return new Result<CategoryResponseDto> { Success = false, StatusCode = "404", Message = "Category not found" };

            var adminCheck = await ValidateAdminAsync(dto.AdminId, dto.SchoolId);
            if (!adminCheck.Ok) return new Result<CategoryResponseDto> { Success = false, StatusCode = "403", Message = adminCheck.Message };

            if (await _db.Categories.AnyAsync(x => x.Name == dto.Name && x.Id != id && x.SchoolId == dto.SchoolId))
                return new Result<CategoryResponseDto> { Success = false, StatusCode = "409", Message = "Category name exists" };

            // update scalar fields
            c.Name = dto.Name;
            c.IsActive = dto.IsActive;

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                await _db.SaveChangesAsync();

                // sync mapping: compute toAdd and toRemove
                var existingMaps = await _db.CategoryCategoryAttributes.Where(m => m.CategoryId == id).ToListAsync();
                var existingAttrIds = existingMaps.Select(m => m.CategoryAttributeId).ToHashSet();
                var incoming = dto.AttributeIds.Distinct().ToHashSet();

                var toAdd = incoming.Except(existingAttrIds).ToList();
                var toRemove = existingAttrIds.Except(incoming).ToList();

                // validate attributes to add
                foreach (var aid in toAdd)
                {
                    var ok = await _db.CategoryAttributes.AnyAsync(a => a.Id == aid && a.SchoolId == dto.SchoolId);
                    if (!ok)
                    {
                        await tx.RollbackAsync();
                        return new Result<CategoryResponseDto> { Success = false, StatusCode = "422", Message = $"Attribute {aid} not found for school." };
                    }
                    _db.CategoryCategoryAttributes.Add(new CategoryCategoryAttribute { CategoryId = id, CategoryAttributeId = aid, CreatedAt = DateTime.UtcNow });
                }

                if (toRemove.Count > 0)
                {
                    var removeEntities = existingMaps.Where(m => toRemove.Contains(m.CategoryAttributeId)).ToList();
                    _db.CategoryCategoryAttributes.RemoveRange(removeEntities);
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Update category failed");
                return new Result<CategoryResponseDto> { Success = false, StatusCode = "500", Message = "Failed to update category" };
            }

            return await GetByIdAsync(id) as Result<CategoryResponseDto>;
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c == null) return new Result<bool> { Success = false, StatusCode = "404", Message = "Category not found", Data = false };

            var adminCheck = await ValidateAdminAsync(c.AdminId, c.SchoolId);
            if (!adminCheck.Ok) return new Result<bool> { Success = false, StatusCode = "403", Message = adminCheck.Message, Data = false };

            try
            {
                // maps will be cascade deleted if configured; otherwise remove explicitly
                var maps = _db.CategoryCategoryAttributes.Where(m => m.CategoryId == id);
                _db.CategoryCategoryAttributes.RemoveRange(maps);

                _db.Categories.Remove(c);
                await _db.SaveChangesAsync();

                return new Result<bool> { Success = true, StatusCode = "200", Message = "Category deleted", Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete category failed");
                return new Result<bool> { Success = false, StatusCode = "500", Message = "Failed to delete category", Data = false };
            }
        }
    }
}
