using EStore.Entity.DTO.CategoryAttribute;
using EStore.Services.Common.Behaviors;
using EStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryAttributesController : ControllerBase
    {
        private readonly ICategoryAttributeService _service;

        public CategoryAttributesController(ICategoryAttributeService service)
        {
            _service = service;
        }

        // ---------------------------------------------------------
        // GET ALL BY SCHOOL
        // ---------------------------------------------------------
        [HttpGet("GetAll/{schoolId:int}")]
        public async Task<ActionResult<Result<IEnumerable<CategoryAttributeResponseDto>>>> GetAll(int schoolId)
        {
            var res = await _service.GetAllAsync(schoolId);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        // ---------------------------------------------------------
        // GET BY ID
        // ---------------------------------------------------------
        [HttpGet("GetById/{id:int}")]
        public async Task<ActionResult<Result<CategoryAttributeResponseDto?>>> GetById(int id)
        {
            var res = await _service.GetByIdAsync(id);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        // ---------------------------------------------------------
        // CREATE ATTRIBUTE
        // ---------------------------------------------------------
        [HttpPost("Create")]
        public async Task<ActionResult<Result<CategoryAttributeResponseDto>>> Create([FromBody] CategoryAttributeCreateDto dto)
        {
            var res = await _service.CreateAsync(dto);
            return StatusCode(int.Parse(res.StatusCode ?? "201"), res);
        }

        // ---------------------------------------------------------
        // UPDATE ATTRIBUTE
        // ---------------------------------------------------------
        [HttpPut("Update/{id:int}")]
        public async Task<ActionResult<Result<CategoryAttributeResponseDto>>> Update(int id, [FromBody] CategoryAttributeUpdateDto dto)
        {
            var res = await _service.UpdateAsync(id, dto);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        // ---------------------------------------------------------
        // DELETE ATTRIBUTE
        // ---------------------------------------------------------
        [HttpDelete("Delete/{id:int}")]
        public async Task<ActionResult<Result<bool>>> Delete(int id)
        {
            var res = await _service.DeleteAsync(id);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }
    }
}
