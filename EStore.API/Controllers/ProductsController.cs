using EStore.Entity.DTO.Product;
using EStore.Services.Common.Behaviors;
using EStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        // ============================================================
        //  GET ALL PRODUCTS BY SCHOOL
        // ============================================================
        [HttpGet("school/{schoolId}")]
        public async Task<ActionResult<Result<IEnumerable<ProductResponseDto>>>> GetAll(int schoolId)
        {
            var res = await _service.GetAllAsync(schoolId);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        // ============================================================
        //  GET PRODUCT BY ID
        // ============================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Result<ProductResponseDto>>> Get(int id)
        {
            var res = await _service.GetAsync(id);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        // ============================================================
        //  CREATE PRODUCT
        // ============================================================
        [HttpPost("CreateProduct")]
        [Consumes("multipart/form-data")]    // Important for images
        public async Task<ActionResult<Result<ProductResponseDto>>> Create(
            [FromForm] ProductCreateDto dto)
        {
            var res = await _service.CreateAsync(dto);
            return StatusCode(int.Parse(res.StatusCode ?? "201"), res);
        }

        // ============================================================
        //  UPDATE PRODUCT
        // ============================================================
        [HttpPut("UpdateProduct/{id:int}")]
        [Consumes("multipart/form-data")]    // Important for images
        public async Task<ActionResult<Result<ProductResponseDto>>> Update(
            int id,
            [FromForm] ProductUpdateDto dto)
        {
            var res = await _service.UpdateAsync(id, dto);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }

        // ============================================================
        //  DELETE PRODUCT
        // ============================================================
        [HttpDelete("DeleteProduct/{id:int}")]
        public async Task<ActionResult<Result<bool>>> Delete(int id)
        {
            var res = await _service.DeleteAsync(id);
            return StatusCode(int.Parse(res.StatusCode ?? "200"), res);
        }
    }
}
