using EStore.Entity.Common.Enums;
using EStore.Entity.DTO.Category;
using EStore.Entity.Models;
using EStore.Services.Common.Behaviors;
using EStore.Services.Common.Constant;
using EStore.Services.Interfaces;
using EStore.Services.Interfaces.GenericClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private readonly IWebHostEnvironment _env;

        public CategoryService(
            ApplicationDbContext db,
            ILogger<CategoryService> logger,
            IGenericUserClientRepository userClient,
            IWebHostEnvironment env)
        {
            _db = db;
            _logger = logger;
            _userClient = userClient;
            _env = env;
        }

        //  VALIDATE ADMIN (Safe JSON parsing)
        private async Task<(bool Ok, string Message)> ValidateAdminAsync(Guid adminId, int schoolId)
        {
            if (adminId == Guid.Empty)
                return (false, "AdminId is required.");

            try
            {
                var json = await _userClient.GetAsync<JsonElement>(
                    $"{ApiConstants.GetUserDetails}/{adminId}");

                _logger.LogWarning("RAW ADMIN JSON => " + json.ToString());

                // Check success = true
                if (!json.TryGetProperty("success", out var successElem) ||
                    !successElem.GetBoolean())
                    return (false, "Admin not found in SMS.");

                // Extract data
                if (!json.TryGetProperty("data", out var dataElem))
                    return (false, "SMS response missing data.");

                // Extract school id
                if (!dataElem.TryGetProperty("schoolId", out var schoolElem) ||
                    schoolElem.ValueKind != JsonValueKind.Number)
                    return (false, "Invalid or missing schoolId in SMS response.");

                int adminSchool = schoolElem.GetInt32();

                if (adminSchool != schoolId)
                    return (false, $"Admin belongs to school {adminSchool}, not {schoolId}.");

                // Extract roles
                if (!dataElem.TryGetProperty("roles", out var rolesElem) ||
                    rolesElem.ValueKind != JsonValueKind.Array)
                    return (false, "Roles missing in SMS response.");

                List<string> roles = rolesElem.EnumerateArray()
                                              .Where(r => r.ValueKind == JsonValueKind.String)
                                              .Select(r => r.GetString())
                                              .ToList();

                var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "super_admin",
                    "store_admin"
                };

                if (!roles.Any(r => allowed.Contains(r)))
                    return (false, "Admin does not have permission.");

                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin validation failed.");
                return (false, "Failed to validate admin user.");
            }
        }

        // IMAGE UPLOAD
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine("uploads", "categories",
                DateTime.UtcNow.Year.ToString(),
                DateTime.UtcNow.Month.ToString("00"));

            Directory.CreateDirectory(Path.Combine(root, folder));

            var filename = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(root, folder, filename);

            await using var fs = File.Create(path);
            await file.CopyToAsync(fs);

            return "/" + Path.Combine(folder, filename).Replace("\\", "/");
        }

        //  GET ALL
        public async Task<Result<IEnumerable<CategoryResponseDto>>> GetAllAsync(int schoolId)
        {
            var list = await _db.Categories
                .AsNoTracking()
                .Where(c => c.SchoolId == schoolId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return new Result<IEnumerable<CategoryResponseDto>>
            {
                Success = true,
                StatusCode = "200",
                Message = "Categories retrieved",
                Data = list.Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    SchoolId = c.SchoolId,
                    AdminId = c.AdminId,
                    Name = c.Name,
                    HasSizeVariants = c.HasSizeVariants,
                    SizeType = c.SizeType,
                    AvailableSizes = c.AvailableSizes,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
            };
        }

        
        //  GET BY ID
        public async Task<Result<CategoryResponseDto?>> GetByIdAsync(int id)
        {
            var c = await _db.Categories.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null)
                return new Result<CategoryResponseDto?>
                {
                    Success = false,
                    StatusCode = "404",
                    Message = "Category not found"
                };

            return new Result<CategoryResponseDto?>
            {
                Success = true,
                StatusCode = "200",
                Data = new CategoryResponseDto
                {
                    Id = c.Id,
                    SchoolId = c.SchoolId,
                    AdminId = c.AdminId,
                    Name = c.Name,
                    HasSizeVariants = c.HasSizeVariants,
                    SizeType = c.SizeType,
                    AvailableSizes = c.AvailableSizes,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                }
            };
        }

        // CREATE
        public async Task<Result<CategoryResponseDto>> CreateAsync(CategoryCreateDto dto)
        {
            var check = await ValidateAdminAsync(dto.AdminId, dto.SchoolId);
            if (!check.Ok)
                return new Result<CategoryResponseDto>
                {
                    Success = false,
                    StatusCode = "403",
                    Message = check.Message
                };

            if (await _db.Categories.AnyAsync(c => c.Name == dto.Name && c.SchoolId == dto.SchoolId))
                return new Result<CategoryResponseDto>
                {
                    Success = false,
                    StatusCode = "409",
                    Message = "Category already exists"
                };

            // ****************************
            // SIZE VALIDATION LOGIC
            // ****************************
            if (!dto.HasSizeVariants)
            {
                dto.SizeType = SizeType.None;
                dto.AvailableSizes = null;
            }
            else
            {
                if (dto.SizeType == SizeType.None)
                    return new Result<CategoryResponseDto>
                    {
                        Success = false,
                        StatusCode = "422",
                        Message = "SizeType is required when HasSizeVariants = true"
                    };

                if (string.IsNullOrWhiteSpace(dto.AvailableSizes))
                    return new Result<CategoryResponseDto>
                    {
                        Success = false,
                        StatusCode = "422",
                        Message = "AvailableSizes is required when HasSizeVariants = true"
                    };
            }
            // ****************************

            string? imageUrl = dto.Image != null ? await SaveImageAsync(dto.Image) : null;

            var entity = new Category
            {
                SchoolId = dto.SchoolId,
                AdminId = dto.AdminId,
                Name = dto.Name,
                HasSizeVariants = dto.HasSizeVariants,
                SizeType = dto.SizeType,
                AvailableSizes = dto.AvailableSizes,
                ImageUrl = imageUrl
            };

            _db.Categories.Add(entity);
            await _db.SaveChangesAsync();

            return new Result<CategoryResponseDto>
            {
                Success = true,
                StatusCode = "201",
                Message = "Category created",
                Data = new CategoryResponseDto
                {
                    Id = entity.Id,
                    SchoolId = entity.SchoolId,
                    AdminId = entity.AdminId,
                    Name = entity.Name,
                    HasSizeVariants = entity.HasSizeVariants,
                    SizeType = entity.SizeType,
                    AvailableSizes = entity.AvailableSizes,
                    ImageUrl = entity.ImageUrl,
                    IsActive = entity.IsActive,
                    CreatedAt = entity.CreatedAt
                }
            };
        }


        // UPDATE
        public async Task<Result<CategoryResponseDto>> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null)
                return new Result<CategoryResponseDto>
                {
                    Success = false,
                    StatusCode = "404",
                    Message = "Category not found"
                };

            var check = await ValidateAdminAsync(c.AdminId, c.SchoolId);
            if (!check.Ok)
                return new Result<CategoryResponseDto>
                {
                    Success = false,
                    StatusCode = "403",
                    Message = check.Message
                };

            if (await _db.Categories.AnyAsync(x => x.Name == dto.Name && x.Id != id))
                return new Result<CategoryResponseDto>
                {
                    Success = false,
                    StatusCode = "409",
                    Message = "Category name already exists"
                };

            // ****************************
            // SIZE VALIDATION LOGIC
            // ****************************
            if (!dto.HasSizeVariants)
            {
                dto.SizeType = SizeType.None;
                dto.AvailableSizes = null;
            }
            else
            {
                if (dto.SizeType == SizeType.None)
                    return new Result<CategoryResponseDto>
                    {
                        Success = false,
                        StatusCode = "422",
                        Message = "SizeType is required when HasSizeVariants = true"
                    };

                if (string.IsNullOrWhiteSpace(dto.AvailableSizes))
                    return new Result<CategoryResponseDto>
                    {
                        Success = false,
                        StatusCode = "422",
                        Message = "AvailableSizes is required when HasSizeVariants = true"
                    };
            }
            // ****************************

            // IMAGE REPLACE
            if (dto.NewImage != null)
            {
                var newUrl = await SaveImageAsync(dto.NewImage);

                if (!string.IsNullOrWhiteSpace(c.ImageUrl))
                {
                    var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var oldPath = Path.Combine(root, c.ImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (File.Exists(oldPath))
                    {
                        try { File.Delete(oldPath); } catch { }
                    }
                }

                c.ImageUrl = newUrl;
            }

            // Update normal fields
            c.Name = dto.Name;
            c.HasSizeVariants = dto.HasSizeVariants;
            c.SizeType = dto.SizeType;
            c.AvailableSizes = dto.AvailableSizes;
            c.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();

            return new Result<CategoryResponseDto>
            {
                Success = true,
                StatusCode = "200",
                Message = "Category updated",
                Data = new CategoryResponseDto
                {
                    Id = c.Id,
                    SchoolId = c.SchoolId,
                    AdminId = c.AdminId,
                    Name = c.Name,
                    HasSizeVariants = c.HasSizeVariants,
                    SizeType = c.SizeType,
                    AvailableSizes = c.AvailableSizes,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                }
            };
        }



        //  DELETE

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null)
                return new Result<bool>
                {
                    Success = false,
                    StatusCode = "404",
                    Message = "Category not found",
                    Data = false
                };

            var check = await ValidateAdminAsync(c.AdminId, c.SchoolId);
            if (!check.Ok)
                return new Result<bool>
                {
                    Success = false,
                    StatusCode = "403",
                    Message = check.Message,
                    Data = false
                };

            _db.Categories.Remove(c);
            await _db.SaveChangesAsync();

            return new Result<bool>
            {
                Success = true,
                StatusCode = "200",
                Message = "Category deleted",
                Data = true
            };
        }
    }
}
