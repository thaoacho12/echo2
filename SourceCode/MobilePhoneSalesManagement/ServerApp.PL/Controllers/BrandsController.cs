using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Exceptions;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
namespace ServerApp.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet("get-all-brands")]
        public async Task<ActionResult<IEnumerable<BrandVm>>> GetBrands()
        {
            var result = await _brandService.GetAllBrandAsync();
            return Ok(result); // 200 OK nếu có dữ liệu.
        }

        [HttpGet("get-all-brands-by-page")]
        public async Task<ActionResult<IEnumerable<BrandVm>>> GetBrands([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortField = "updatedDate", [FromQuery] bool orderBy = true)
        {
            var result = await _brandService.GetAllBrandAsync(pageNumber, pageSize, filter: null, sortField, orderBy);
            return Ok(result); // 200 OK nếu có dữ liệu.
        }
        [HttpGet("search-brands-by-page")]
        public async Task<ActionResult<IEnumerable<BrandVm>>> GetBrands([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "", [FromQuery] string sortField = "updatedDate", [FromQuery] bool orderBy = true)
        {
            var result = await _brandService.GetAllBrandAsync(pageNumber, pageSize, search, sortField, orderBy);
            return Ok(result); // 200 OK nếu có dữ liệu.
        }
        [HttpGet("filter-brands-by-page")]
        public async Task<ActionResult<IEnumerable<BrandVm>>> GetBrands([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] bool filter = true, [FromQuery] string sortField = "updatedDate", [FromQuery] bool orderBy = true)
        {
            var result = await _brandService.GetAllBrandAsync(pageNumber, pageSize, b => b.IsActive == filter, sortField, orderBy);
            return Ok(result); // 200 OK nếu có dữ liệu.
        }
        [HttpGet("get-brand-by-id/{id}")]
        public async Task<ActionResult<BrandVm>> GetBrand(int id)
        {
            var result = await _brandService.GetByBrandIdAsync(id);

            if (result == null)
            {
                return NotFound(new { Message = $"Brand with ID {id} not found." }); // 404 Not Found nếu không tìm thấy.
            }

            return Ok(result); // 200 OK nếu tìm thấy.
        }

        [HttpPost("add-new-brand")]
        public async Task<ActionResult<BrandVm>> PostBrand(InputBrandVm brandVm)
        {
            var result = await _brandService.AddBrandAsync(brandVm);

            if (result == null)
            {
                return BadRequest(new { Message = "Failed to create the brand." }); // 400 Bad Request nếu không tạo được.
            }

            return CreatedAtAction(nameof(GetBrand), new { id = result.BrandId }, result); // 201 Created nếu tạo thành công.
        }

        [HttpPut("update-brand/{id}")]
        public async Task<IActionResult> PutBrand(int id, InputBrandVm brandVm)
        {
            var result = await _brandService.UpdateBrandAsync(id, brandVm);
            if (result == null)
            {
                return NotFound(new { Message = $"Brand with ID {id} not found." }); // 404 Not Found nếu không tìm thấy.
            }

            return Ok(result); // 200 OK nếu cập nhật thành công.
        }

        [HttpDelete("delete-brand-by-id/{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var result = await _brandService.DeleteBrandAsync(id);
            if (result == null)
            {
                return NotFound(new { Message = $"Brand with ID {id} not found." }); // 404 Not Found nếu không tìm thấy.
            }

            return NoContent();
        }
        [HttpDelete("delete-multiple-brand")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> brandIds)
        {
            if (brandIds == null || brandIds.Count == 0)
            {
                return BadRequest("Brand IDs must be provided.");
            }

            try
            {
                // Gọi phương thức xóa hàng loạt từ service
                var (deletedCount, updateCount) = await _brandService.DeleteMultipleAsync(
                    brandIds,
                    brand => brand.IsActive == false,
                    async brand =>
                    {
                        brand.IsActive = false;
                        await _brandService.UpdateBrandAsync(brand);
                    });

                if (deletedCount + updateCount > 0)
                {
                    return Ok(new { Message = "Brands deleted successfully.", DeletedCount = deletedCount, UpdateCount = updateCount });
                }

                return NotFound("No brands were deleted.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpPut("restore-multiple-brand")]
        public async Task<IActionResult> UpdateMultiple([FromBody] List<int> brandIds)
        {
            if (brandIds == null || brandIds.Count == 0)
            {
                return BadRequest("Brand IDs must be provided.");
            }

            try
            {
                // Gọi phương thức xóa hàng loạt từ service
                var updateCount = await _brandService.RestoreMultipleAsync(
                    brandIds,
                    brand => brand.IsActive == false,
                    async brand =>
                    {
                        brand.IsActive = true;
                        await _brandService.UpdateBrandAsync(brand);
                    });

                if (updateCount > 0)
                {
                    return Ok(new { Message = "Brands restore successfully.", UpdateCount = updateCount });
                }

                return NotFound("No brands were restore.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
