using EStore.Services.Common.Behaviors;
using EStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _svc;
        public CategoriesController(ICategoryService svc) => _svc = svc;

        [HttpGet("get-all/{schoolId}")]
        public async Task<ActionResult<Result<IEnumerable<CategoryResponseDto>>>> GetAll(int schoolId)
        {
            var res = await _svc.GetAllAsync(schoolId);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        [HttpGet("get/{id:int}")]
        public async Task<ActionResult<Result<CategoryResponseDto?>>> Get(int id)
        {
            var res = await _svc.GetByIdAsync(id);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Result<CategoryResponseDto>>> Create([FromBody] CategoryCreateDto dto)
        {
            var res = await _svc.CreateAsync(dto);
            return StatusCode(int.Parse(res.StatusCode ?? "201"), res);
        }

        [HttpPut("update/{id:int}")]
        public async Task<ActionResult<Result<CategoryResponseDto>>> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            var res = await _svc.UpdateAsync(id, dto);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<ActionResult<Result<bool>>> Delete(int id)
        {
            var res = await _svc.DeleteAsync(id);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }
    }
}
