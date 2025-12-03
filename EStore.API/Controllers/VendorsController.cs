using EStore.Entity.DTO.Vendor;
using EStore.Services.Common.Behaviors;
using EStore.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorsController : ControllerBase
    {
        private readonly IVendorService _service;

        public VendorsController(IVendorService service)
        {
            _service = service;
        }

        [HttpGet("school/{schoolId}")]
        public async Task<ActionResult<Result<IEnumerable<VendorResponseDto>>>> GetAll(int schoolId)
        {
            var res = await _service.GetAllAsync(schoolId);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        [HttpGet("GetVendorById/{id:int}")]
        public async Task<ActionResult<Result<VendorResponseDto?>>> GetById(int id)
        {
            var res = await _service.GetByIdAsync(id);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        [HttpPost("CreateVendor")]
        public async Task<ActionResult<Result<VendorResponseDto>>> Create([FromBody] VendorCreateDto dto)
        {
            var res = await _service.CreateAsync(dto);
            return StatusCode(int.Parse(res.StatusCode ?? "201"), res);
        }

        [HttpPut("UpdateVendor/{id:int}")]
        public async Task<ActionResult<Result<VendorResponseDto>>> Update(int id, [FromBody] VendorUpdateDto dto)
        {
            var res = await _service.UpdateAsync(id, dto);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        [HttpDelete("DeleteVendor/{id:int}")]
        public async Task<ActionResult<Result<bool>>> Delete(int id)
        {
            var res = await _service.DeleteAsync(id);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }
    }
}
