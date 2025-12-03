using EStore.Entity.DTO.Product;
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
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ProductService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IGenericUserClientRepository _userClient;

        public ProductService(
            ApplicationDbContext db,
            ILogger<ProductService> logger,
            IWebHostEnvironment env,
            IGenericUserClientRepository userClient)
        {
            _db = db;
            _logger = logger;
            _env = env;
            _userClient = userClient;
        }

        // =======================================================================
        // VALIDATE ADMIN (same style used in Category)
        // =======================================================================
        private async Task<(bool Ok, string Message)> ValidateAdminAsync(Guid adminId, int schoolId)
        {
            try
            {
                var json = await _userClient.GetAsync<JsonElement>(
                    $"{ApiConstants.GetUserDetails}/{adminId}");

                if (!json.TryGetProperty("success", out var successElem) || !successElem.GetBoolean())
                    return (false, "Admin not found.");

                if (!json.TryGetProperty("data", out var dataElem))
                    return (false, "Invalid response from SMS.");

                int adminSchool = dataElem.GetProperty("schoolId").GetInt32();
                if (adminSchool != schoolId)
                    return (false, "Admin not part of this school.");

                var roles = dataElem.GetProperty("roles")
                    .EnumerateArray()
                    .Select(r => r.GetString())
                    .ToList();

                var allowed = new[] { "super_admin", "store_admin" };

                if (!roles.Any(r => allowed.Contains(r, StringComparer.OrdinalIgnoreCase)))
                    return (false, "Admin has no permission for product operations.");

                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin validation failed.");
                return (false, "Admin validation failed.");
            }
        }

        // =======================================================================
        // AUTO PRODUCT CODE GENERATOR (PR001 → PR002) PER SCHOOL
        // =======================================================================
        private async Task<string> GenerateProductCodeAsync(int schoolId)
        {
            var last = await _db.Products
                .Where(x => x.SchoolId == schoolId)
                .OrderByDescending(x => x.Id)
                .Select(x => x.ProductCode)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(last))
                return "PR001";

            var numPart = last.Substring(2);

            if (!int.TryParse(numPart, out int number))
                number = 0;

            number++;
            return $"PR{number:000}";
        }

        // =======================================================================
        // CREATE PRODUCT
        // =======================================================================
        public async Task<Result<ProductResponseDto>> CreateAsync(ProductCreateDto dto)
        {
            var adminCheck = await ValidateAdminAsync(dto.AdminId, dto.SchoolId);
            if (!adminCheck.Ok)
                return new Result<ProductResponseDto>
                {
                    Success = false,
                    StatusCode = "403",
                    Message = adminCheck.Message
                };

            // Validate Category
            var category = await _db.Categories
                .FirstOrDefaultAsync(x => x.Id == dto.CategoryId && x.SchoolId == dto.SchoolId);

            if (category == null)
                return new Result<ProductResponseDto>
                {
                    Success = false,
                    StatusCode = "404",
                    Message = "Category not found"
                };

            // ⭐⭐⭐ NEW VALIDATION BLOCK ⭐⭐⭐
            // SIMPLE & CLEAN VALIDATION
            if (dto.HasVariants)
            {
                // Must provide variants
                if (dto.Variants == null || dto.Variants.Count == 0)
                {
                    return new Result<ProductResponseDto>
                    {
                        Success = false,
                        StatusCode = "422",
                        Message = "At least one variant is required when HasVariants = true"
                    };
                }

                // Simple fields can be provided or empty – we don't validate them
            }
            else
            {
                // Simple product must have prices & quantity
                if (dto.MRP == null || dto.PurchasePrice == null || dto.Quantity == null)
                {
                    return new Result<ProductResponseDto>
                    {
                        Success = false,
                        StatusCode = "422",
                        Message = "MRP, PurchasePrice, and Quantity are required when HasVariants = false"
                    };
                }

                // Variants ignored
                dto.Variants = null;
            }
            // ⭐⭐⭐ END VALIDATION ⭐⭐⭐

            string productCode = await GenerateProductCodeAsync(dto.SchoolId);

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var product = new Product
                {
                    AdminId = dto.AdminId,
                    SchoolId = dto.SchoolId,
                    CategoryId = dto.CategoryId,

                    Name = dto.Name,
                    ProductCode = productCode,
                    HasVariants = dto.HasVariants,

                    MRP = dto.MRP,
                    PurchasePrice = dto.PurchasePrice,
                    Quantity = dto.Quantity,
                    ReorderLevel = dto.ReorderLevel,

                    Remarks = dto.Remarks
                };

                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                // Save variants
                if (dto.HasVariants && dto.Variants != null)
                {
                    foreach (var v in dto.Variants)
                    {
                        _db.ProductVariants.Add(new ProductVariant
                        {
                            ProductId = product.Id,
                            AttributeId = v.AttributeId,
                            AttributeValue = v.AttributeValue,
                            MRP = v.MRP,
                            PurchasePrice = v.PurchasePrice,
                            Quantity = v.Quantity,
                            ReorderLevel = v.ReorderLevel
                        });
                    }
                    await _db.SaveChangesAsync();
                }

                // Save images
                if (dto.Images != null && dto.Images.Count > 0)
                {
                    await SaveImagesAsync(product.Id, dto.SchoolId, dto.Images);
                }

                await tx.CommitAsync();

                return await GetAsync(product.Id)
                    as Result<ProductResponseDto>
                    ?? new Result<ProductResponseDto> { Success = false, Message = "Product created but fetch failed" };
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Product creation failed.");

                return new Result<ProductResponseDto>
                {
                    Success = false,
                    StatusCode = "500",
                    Message = "Failed to create product"
                };
            }
        }

        // SAVE IMAGES TO /wwwroot/uploads/products/schoolId/productId/
        private async Task SaveImagesAsync(int productId, int schoolId, List<IFormFile> images)
        {
            string basePath = Path.Combine(_env.WebRootPath, "uploads", "products", schoolId.ToString(), productId.ToString());

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            foreach (var img in images)
            {
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
                string filePath = Path.Combine(basePath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await img.CopyToAsync(stream);

                _db.ProductImages.Add(new ProductImage
                {
                    ProductId = productId,
                    FileName = fileName,
                    ImageUrl = $"/uploads/products/{schoolId}/{productId}/{fileName}"
                });
            }

            await _db.SaveChangesAsync();
        }

        // =======================================================================
        // GET PRODUCT BY ID
        // =======================================================================
        public async Task<Result<ProductResponseDto?>> GetAsync(int id)
        {
            var product = await _db.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return new Result<ProductResponseDto?>()
                {
                    Success = false,
                    StatusCode = "404",
                    Message = "Product not found"
                };

            var category = await _db.Categories.FindAsync(product.CategoryId);

            var dto = new ProductResponseDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = category?.Name ?? "",
                HasVariants = product.HasVariants,
                Status = product.IsActive ? "Active" : "Inactive",
                Remarks = product.Remarks,
                MRP = product.MRP,
                PurchasePrice = product.PurchasePrice,
                Quantity = product.Quantity,
                ReorderLevel = product.ReorderLevel,

                Images = product.Images.Select(i => i.ImageUrl).ToList(),
                Variants = product.Variants.Select(v => new ProductVariantResponseDto
                {
                    Id = v.Id,
                    AttributeId = v.AttributeId,
                    AttributeValue = v.AttributeValue,
                    MRP = v.MRP,
                    PurchasePrice = v.PurchasePrice,
                    Quantity = v.Quantity,
                    ReorderLevel = v.ReorderLevel,
                    AttributeName = _db.CategoryAttributes
                        .Where(a => a.Id == v.AttributeId)
                        .Select(a => a.Name)
                        .FirstOrDefault() ?? ""
                }).ToList()
            };

            return new Result<ProductResponseDto?>()
            {
                Success = true,
                StatusCode = "200",
                Data = dto
            };
        }

        // =======================================================================
        // GET ALL PRODUCTS (school wise)
        // =======================================================================
        public async Task<Result<IEnumerable<ProductResponseDto>>> GetAllAsync(int schoolId)
        {
            var list = await _db.Products
                .Where(x => x.SchoolId == schoolId)
                .Include(x => x.Images)
                .Include(x => x.Variants)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var result = new List<ProductResponseDto>();

            foreach (var p in list)
            {
                var catName = await _db.Categories
                    .Where(c => c.Id == p.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();

                result.Add(new ProductResponseDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = catName ?? "",
                    HasVariants = p.HasVariants,
                    Status = p.IsActive ? "Active" : "Inactive",
                    MRP = p.MRP,
                    PurchasePrice = p.PurchasePrice,
                    Quantity = p.Quantity,
                    ReorderLevel = p.ReorderLevel,
                    Images = p.Images.Select(i => i.ImageUrl).ToList(),
                    Remarks = p.Remarks
                });
            }

            return new Result<IEnumerable<ProductResponseDto>>
            {
                Success = true,
                StatusCode = "200",
                Data = result
            };
        }

        // =======================================================================
        // UPDATE PRODUCT
        // =======================================================================
        public async Task<Result<ProductResponseDto>> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var product = await _db.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
                return new Result<ProductResponseDto>
                {
                    Success = false,
                    StatusCode = "404",
                    Message = "Product not found"
                };

            var adminCheck = await ValidateAdminAsync(dto.AdminId, dto.SchoolId);
            if (!adminCheck.Ok)
                return new Result<ProductResponseDto> { Success = false, StatusCode = "403", Message = adminCheck.Message };


            // ⭐⭐⭐ NEW VALIDATION BLOCK ⭐⭐⭐
            // SIMPLE & CLEAN VALIDATION
            if (dto.HasVariants)
            {
                // Must provide variants
                if (dto.Variants == null || dto.Variants.Count == 0)
                {
                    return new Result<ProductResponseDto>
                    {
                        Success = false,
                        StatusCode = "422",
                        Message = "At least one variant is required when HasVariants = true"
                    };
                }

                // Simple fields can be provided or empty – we don't validate them
            }
            else
            {
                // Simple product must have prices & quantity
                if (dto.MRP == null || dto.PurchasePrice == null || dto.Quantity == null)
                {
                    return new Result<ProductResponseDto>
                    {
                        Success = false,
                        StatusCode = "422",
                        Message = "MRP, PurchasePrice, and Quantity are required when HasVariants = false"
                    };
                }

                // Variants ignored
                dto.Variants = null;
            }

            // ⭐⭐⭐ END VALIDATION ⭐⭐⭐


            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                product.Name = dto.Name;
                product.HasVariants = dto.HasVariants;
                product.MRP = dto.MRP;
                product.PurchasePrice = dto.PurchasePrice;
                product.Quantity = dto.Quantity;
                product.ReorderLevel = dto.ReorderLevel;
                product.Remarks = dto.Remarks;

                await _db.SaveChangesAsync();

                if (dto.HasVariants)
                {
                    var incomingIds = dto.Variants?.Where(x => x.Id != null).Select(x => x.Id!.Value).ToHashSet() ?? new HashSet<int>();
                    var toRemove = product.Variants.Where(x => !incomingIds.Contains(x.Id)).ToList();
                    _db.ProductVariants.RemoveRange(toRemove);

                    foreach (var v in dto.Variants ?? new List<ProductVariantUpdateDto>())
                    {
                        if (v.Id.HasValue)
                        {
                            var existing = product.Variants.First(x => x.Id == v.Id.Value);
                            existing.AttributeId = v.AttributeId;
                            existing.AttributeValue = v.AttributeValue;
                            existing.MRP = v.MRP;
                            existing.PurchasePrice = v.PurchasePrice;
                            existing.Quantity = v.Quantity;
                            existing.ReorderLevel = v.ReorderLevel;
                        }
                        else
                        {
                            _db.ProductVariants.Add(new ProductVariant
                            {
                                ProductId = id,
                                AttributeId = v.AttributeId,
                                AttributeValue = v.AttributeValue,
                                MRP = v.MRP,
                                PurchasePrice = v.PurchasePrice,
                                Quantity = v.Quantity,
                                ReorderLevel = v.ReorderLevel
                            });
                        }
                    }

                    await _db.SaveChangesAsync();
                }

                if (dto.RemoveImageIds != null)
                {
                    var remove = product.Images.Where(i => dto.RemoveImageIds.Contains(i.Id)).ToList();
                    foreach (var img in remove)
                    {
                        var path = Path.Combine(_env.WebRootPath, img.ImageUrl.TrimStart('/'));
                        if (File.Exists(path))
                            File.Delete(path);

                        _db.ProductImages.Remove(img);
                    }
                    await _db.SaveChangesAsync();
                }

                if (dto.NewImages != null)
                {
                    await SaveImagesAsync(product.Id, product.SchoolId, dto.NewImages);
                }

                await tx.CommitAsync();
                return await GetAsync(product.Id) as Result<ProductResponseDto>;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Update failed");
                return new Result<ProductResponseDto> { Success = false, StatusCode = "500", Message = "Failed to update" };
            }
        }


        // =======================================================================
        // DELETE PRODUCT
        // =======================================================================
        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var p = await _db.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null)
                return new Result<bool> { Success = false, StatusCode = "404", Message = "Product not found" };

            using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                // delete folder
                string folder = Path.Combine(_env.WebRootPath, "uploads", "products", p.SchoolId.ToString(), p.Id.ToString());
                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);

                _db.ProductVariants.RemoveRange(p.Variants);
                _db.ProductImages.RemoveRange(p.Images);
                _db.Products.Remove(p);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return new Result<bool> { Success = true, StatusCode = "200", Message = "Product deleted", Data = true };
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Product delete failed");
                return new Result<bool> { Success = false, StatusCode = "500", Message = "Failed to delete", Data = false };
            }
        }
    }
}
