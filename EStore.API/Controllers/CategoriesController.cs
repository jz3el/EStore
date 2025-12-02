using EStore.Entity.DTO.Category;
using EStore.Services.Common.Behaviors;
using EStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet("school/{schoolId}")]
        public async Task<ActionResult<Result<IEnumerable<CategoryResponseDto>>>> GetAll(int schoolId)
        {
            var res = await _service.GetAllAsync(schoolId);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        [HttpGet("GetCategoryById/{id:int}")]
        public async Task<ActionResult<Result<CategoryResponseDto?>>> GetById(int id)
        {
            var res = await _service.GetByIdAsync(id);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        [HttpPost("CreateCategory")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Result<CategoryResponseDto>>> Create([FromForm] CategoryCreateDto dto)
        {
            var res = await _service.CreateAsync(dto);
            return StatusCode(int.Parse(res.StatusCode ?? "201"), res);
        }

        [HttpPut("UpdateCategory/{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Result<CategoryResponseDto>>> Update(int id, [FromForm] CategoryUpdateDto dto)
        {
            var res = await _service.UpdateAsync(id, dto);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        [HttpDelete("DeleteCategory/{id:int}")]
        public async Task<ActionResult<Result<bool>>> Delete(int id)
        {
            var res = await _service.DeleteAsync(id);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }
    }
}
