using EStore.Entity.DTO.Vendor;
using EStore.Entity.Models;
using EStore.Services.Common.Behaviors;
using EStore.Services.Common.Constant;
using EStore.Services.Interfaces;
using EStore.Services.Interfaces.GenericClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EStore.Services.Repositories
{
    public class VendorService : IVendorService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<VendorService> _logger;
        private readonly IGenericUserClientRepository _userClient;

        public VendorService(
            ApplicationDbContext db,
            ILogger<VendorService> logger,
            IGenericUserClientRepository userClient)
        {
            _db = db;
            _logger = logger;
            _userClient = userClient;
        }

        // ADMIN VALIDATION
        private async Task<(bool Ok, string Message)> ValidateAdminAsync(Guid adminId, int schoolId)
        {
            if (adminId == Guid.Empty)
                return (false, "AdminId is required.");

            try
            {
                var json = await _userClient.GetAsync<JsonElement>(
                    $"{ApiConstants.GetUserDetails}/{adminId}");

                if (!json.TryGetProperty("success", out var successElem) ||
                    !successElem.GetBoolean())
                    return (false, "Admin not found.");

                if (!json.TryGetProperty("data", out var dataElem))
                    return (false, "Invalid SMS response.");

                if (!dataElem.TryGetProperty("schoolId", out var schoolElem) ||
                    schoolElem.ValueKind != JsonValueKind.Number)
                    return (false, "SchoolId missing.");

                int adminSchool = schoolElem.GetInt32();
                if (adminSchool != schoolId)
                    return (false, $"Admin belongs to school {adminSchool}, not {schoolId}.");

                if (!dataElem.TryGetProperty("roles", out var rolesElem) ||
                    rolesElem.ValueKind != JsonValueKind.Array)
                    return (false, "Roles missing.");

                var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "super_admin", "store_admin" };

                var roles = rolesElem.EnumerateArray()
                                     .Where(r => r.ValueKind == JsonValueKind.String)
                                     .Select(r => r.GetString())
                                     .ToList();

                if (!roles.Any(allowed.Contains))
                    return (false, "Admin does not have permission.");

                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin validation failed");
                return (false, "Failed to validate admin.");
            }
        }

        // GET ALL
        public async Task<Result<IEnumerable<VendorResponseDto>>> GetAllAsync(int schoolId)
        {
            var list = await _db.Vendors
                .AsNoTracking()
                .Where(v => v.SchoolId == schoolId)
                .OrderBy(v => v.CompanyName)
                .ToListAsync();

            return new Result<IEnumerable<VendorResponseDto>>
            {
                Success = true,
                StatusCode = "200",
                Message = "Vendors retrieved",
                Data = list.Select(v => new VendorResponseDto
                {
                    Id = v.Id,
                    SchoolId = v.SchoolId,
                    AdminId = v.AdminId,
                    VendorCode = v.VendorCode,
                    CompanyName = v.CompanyName,
                    GstNumber = v.GstNumber,
                    ContactPerson = v.ContactPerson,
                    Designation = v.Designation,
                    Phone = v.Phone,
                    Email = v.Email,
                    Address = v.Address,
                    BankName = v.BankName,
                    AccountNumber = v.AccountNumber,
                    IfscCode = v.IfscCode,
                    ProductsSupplied = v.ProductsSupplied,
                    IsActive = v.IsActive,
                    CreatedAt = v.CreatedAt
                })
            };
        }

        // GET BY ID
        public async Task<Result<VendorResponseDto?>> GetByIdAsync(int id)
        {
            var v = await _db.Vendors.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (v == null)
                return new Result<VendorResponseDto?>
                { Success = false, StatusCode = "404", Message = "Vendor not found" };

            return new Result<VendorResponseDto?>
            {
                Success = true,
                StatusCode = "200",
                Data = new VendorResponseDto
                {
                    Id = v.Id,
                    SchoolId = v.SchoolId,
                    AdminId = v.AdminId,
                    VendorCode = v.VendorCode,
                    CompanyName = v.CompanyName,
                    GstNumber = v.GstNumber,
                    ContactPerson = v.ContactPerson,
                    Designation = v.Designation,
                    Phone = v.Phone,
                    Email = v.Email,
                    Address = v.Address,
                    BankName = v.BankName,
                    AccountNumber = v.AccountNumber,
                    IfscCode = v.IfscCode,
                    ProductsSupplied = v.ProductsSupplied,
                    IsActive = v.IsActive,
                    CreatedAt = v.CreatedAt
                }
            };
        }

        // CREATE
        public async Task<Result<VendorResponseDto>> CreateAsync(VendorCreateDto dto)
        {
            var check = await ValidateAdminAsync(dto.AdminId, dto.SchoolId);
            if (!check.Ok)
                return new Result<VendorResponseDto>
                { Success = false, StatusCode = "403", Message = check.Message };

            if (await _db.Vendors.AnyAsync(v => v.CompanyName == dto.CompanyName && v.SchoolId == dto.SchoolId))
                return new Result<VendorResponseDto>
                { Success = false, StatusCode = "409", Message = "Vendor already exists" };

            var entity = new Vendor
            {
                SchoolId = dto.SchoolId,
                AdminId = dto.AdminId,
                CompanyName = dto.CompanyName,
                GstNumber = dto.GstNumber,
                ContactPerson = dto.ContactPerson,
                Designation = dto.Designation,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                BankName = dto.BankName,
                AccountNumber = dto.AccountNumber,
                IfscCode = dto.IfscCode,
                ProductsSupplied = dto.ProductsSupplied
            };

            _db.Vendors.Add(entity);
            await _db.SaveChangesAsync();   // Id gets generated

            entity.VendorCode = $"V{entity.Id.ToString().PadLeft(3, '0')}";

            await _db.SaveChangesAsync();   // Save VendorCode update



            return new Result<VendorResponseDto>
            {
                Success = true,
                StatusCode = "201",
                Message = "Vendor created",
                Data = new VendorResponseDto
                {
                    Id = entity.Id,
                    VendorCode = entity.VendorCode,    
                    SchoolId = entity.SchoolId,
                    AdminId = entity.AdminId,
                    CompanyName = entity.CompanyName,
                    GstNumber = entity.GstNumber,
                    ContactPerson = entity.ContactPerson,
                    Designation = entity.Designation,
                    Phone = entity.Phone,
                    Email = entity.Email,
                    Address = entity.Address,
                    BankName = entity.BankName,
                    AccountNumber = entity.AccountNumber,
                    IfscCode = entity.IfscCode,
                    ProductsSupplied = entity.ProductsSupplied,
                    IsActive = entity.IsActive,
                    CreatedAt = entity.CreatedAt
                }
            };
        }

        // UPDATE
        public async Task<Result<VendorResponseDto>> UpdateAsync(int id, VendorUpdateDto dto)
        {
            var v = await _db.Vendors.FirstOrDefaultAsync(x => x.Id == id);
            if (v == null)
                return new Result<VendorResponseDto>
                { Success = false, StatusCode = "404", Message = "Vendor not found" };

            var check = await ValidateAdminAsync(v.AdminId, v.SchoolId);
            if (!check.Ok)
                return new Result<VendorResponseDto>
                { Success = false, StatusCode = "403", Message = check.Message };

            if (await _db.Vendors.AnyAsync(x => x.CompanyName == dto.CompanyName && x.Id != id))
                return new Result<VendorResponseDto>
                { Success = false, StatusCode = "409", Message = "Vendor name already exists" };

            v.CompanyName = dto.CompanyName;
            v.GstNumber = dto.GstNumber;
            v.ContactPerson = dto.ContactPerson;
            v.Designation = dto.Designation;
            v.Phone = dto.Phone;
            v.Email = dto.Email;
            v.Address = dto.Address;
            v.BankName = dto.BankName;
            v.AccountNumber = dto.AccountNumber;
            v.IfscCode = dto.IfscCode;
            v.ProductsSupplied = dto.ProductsSupplied;
            v.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();

            return new Result<VendorResponseDto>
            {
                Success = true,
                StatusCode = "200",
                Message = "Vendor updated",
                Data = new VendorResponseDto
                {
                    Id = v.Id,
                    SchoolId = v.SchoolId,
                    AdminId = v.AdminId,
                    VendorCode = v.VendorCode,
                    CompanyName = v.CompanyName,
                    GstNumber = v.GstNumber,
                    ContactPerson = v.ContactPerson,
                    Designation = v.Designation,
                    Phone = v.Phone,
                    Email = v.Email,
                    Address = v.Address,
                    BankName = v.BankName,
                    AccountNumber = v.AccountNumber,
                    IfscCode = v.IfscCode,
                    ProductsSupplied = v.ProductsSupplied,
                    IsActive = v.IsActive,
                    CreatedAt = v.CreatedAt
                }
            };
        }

        // DELETE
        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var v = await _db.Vendors.FirstOrDefaultAsync(x => x.Id == id);
            if (v == null)
                return new Result<bool>
                { Success = false, StatusCode = "404", Message = "Vendor not found" };

            var check = await ValidateAdminAsync(v.AdminId, v.SchoolId);
            if (!check.Ok)
                return new Result<bool>
                { Success = false, StatusCode = "403", Message = check.Message };

            _db.Vendors.Remove(v);
            await _db.SaveChangesAsync();

            return new Result<bool>
            {
                Success = true,
                StatusCode = "200",
                Message = "Vendor deleted",
                Data = true
            };
        }
    }
}
