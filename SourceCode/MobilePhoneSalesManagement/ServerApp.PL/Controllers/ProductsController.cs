using Microsoft.AspNetCore.Mvc;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.BLL.Services;
using ServerApp.DAL.Models;
using ServerApp.BLL.Services.InterfaceServices;

namespace ServerApp.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("get-all-products")]
        public async Task<ActionResult<IEnumerable<ProductVm>>> GetProducts()
        {
            var result = await _productService.GetAllProductAsync();
            return Ok(result); // 200 OK nếu có dữ liệu.
        }


        [HttpGet("get-all-products-by-page")]
        public async Task<ActionResult<IEnumerable<ProductVm>>> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] string sortField = "updatedDate", [FromQuery] int pageSize = 10, [FromQuery] bool orderBy = false)
        {
            var result = await _productService.GetAllProductAsync(pageNumber, pageSize, sortField, orderBy: orderBy);
            return Ok(result);
        }

        [HttpPost("filter")]
        public async Task<ActionResult<(IEnumerable<ProductVm> Products, int TotalPages)>> FilterProducts([FromBody] FilterRequest filterRequest)
        {
            try
            {
                // Kiểm tra và thiết lập giá trị mặc định cho PageNumber và PageSize nếu không có
                filterRequest.PageNumber ??= 1;   // Mặc định PageNumber là 1 nếu không có
                filterRequest.PageSize ??= 15;     // Mặc định PageSize là 15 nếu không có
                var (products, totalPages) = await _productService.FilterProductsAsync(filterRequest);
                return Ok(new { Products = products, TotalPages = totalPages });
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }

        [HttpGet("get-product-by-id/{id}")]
        public async Task<ActionResult<ProductVm>> GetProduct(int id)
        {
            var result = await _productService.GetByProductIdAsync(id);

            if (result == null)
            {
                return NotFound(new { Message = $"Product with ID {id} not found." }); // 404 Not Found nếu không tìm thấy.
            }

            return Ok(result); // 200 OK nếu tìm thấy.
        }

        [HttpGet("get-specifications-by-product-id/{id}")]
        public async Task<ActionResult<ProductVm>> GetProductSpecificationsByProductIdAsync(int id)
        {
            var result = await _productService.GetProductSpecificationsByProductIdAsync(id);

            if (result == null)
            {
                return NotFound(new { Message = $"Product with ID {id} not found." });
            }

            return Ok(result); // 200 OK nếu tìm thấy.
        }
        [HttpPost("add-new-product")]
        public async Task<ActionResult<ProductVm>> PostProduct(InputProductVm productVm)
        {
            var result = await _productService.AddProductAsync(productVm);

            if (result == null)
            {
                return BadRequest(new { Message = "Failed to create the product." }); // 400 Bad Request nếu không tạo được.
            }

            return CreatedAtAction(nameof(GetProduct), new { id = result.ProductId }, result); // 201 Created nếu tạo thành công.
        }

        [HttpPut("update-product/{id}")]
        public async Task<IActionResult> PutProduct(int id, InputProductVm productVm)
        {
            var result = await _productService.UpdateProductAsync(id, productVm);
            if (result == null)
            {
                return NotFound(new { Message = $"Product with ID {id} not found." });
            }

            return Ok(result);
        }

        [HttpDelete("delete-product-by-id/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (result == null)
            {
                return NotFound(new { Message = $"Product with ID {id} not found." }); // 404 Not Found nếu không tìm thấy.
            }

            return NoContent(); // 204 No Content nếu xóa thành công.
        }
        // Phương thức xóa hàng loạt sản phẩm
        [HttpDelete("delete-multiple-product")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> productIds)
        {
            if (productIds == null || productIds.Count == 0)
            {
                return BadRequest("Product IDs must be provided.");
            }

            try
            {
                // Gọi phương thức xóa hàng loạt từ service
                var (deletedCount, updateCount) = await _productService.DeleteMultipleAsync(
                    productIds,
                    product => product.IsActive == false,
                    async product =>
                    {
                        product.IsActive = false;
                        await _productService.UpdateProductAsync(product);
                    });

                if (deletedCount + updateCount > 0)
                {
                    return Ok(new { Message = "Products deleted successfully.", DeletedCount = deletedCount, UpdateCount = updateCount });
                }

                return NotFound("No products were deleted.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpGet("get-newest-product")]
        public async Task<ActionResult<IEnumerable<ProductVm>>> NewestProducts()
        {
            try
            {
                var newestProducts = await _productService.GetNewestProductsAsync();
                return Ok(newestProducts);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }
        [HttpGet("search-products-by-page")]
        public async Task<ActionResult<IEnumerable<ProductVm>>> GetProducts([FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10, [FromQuery] string search = "", [FromQuery] string sortField = "updatedDate", [FromQuery] bool orderBy = true)
        {
            var result = await _productService.GetAllProductAsync(pageNumber, pageSize, sortField, p => p.Name.Contains(search), orderBy: false);
            return Ok(result);
        }
        [HttpGet("filter-products-by-page")]
        public async Task<ActionResult<IEnumerable<ProductVm>>> GetProducts([FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10, [FromQuery] bool filter = true, [FromQuery] string sortField = "updatedDate", [FromQuery] bool orderBy = true)
        {
            var result = await _productService.GetAllProductAsync(pageNumber, pageSize, sortField, p => p.IsActive == filter, orderBy: false);
            return Ok(result);
        }
        [HttpPut("restore-multiple-product")]
        public async Task<IActionResult> UpdateMultiple([FromBody] List<int> productIds)
        {
            if (productIds == null || productIds.Count == 0)
            {
                return BadRequest("Product IDs must be provided.");
            }

            try
            {
                // Gọi phương thức xóa hàng loạt từ service
                var updateCount = await _productService.RestoreMultipleAsync(
                    productIds,
                    product => product.IsActive == false,
                    async product =>
                    {
                        product.IsActive = true;
                        await _productService.UpdateProductAsync(product);
                    });

                if (updateCount > 0)
                {
                    return Ok(new { Message = "Products restore successfully.", UpdateCount = updateCount });
                }

                return NotFound("No products were restore.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpGet("search-by-name")]
        public async Task<IActionResult> SearchProductsByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest(new { message = "Product name cannot be empty." });
                }

                var products = await _productService.SearchProductsByNameAsync(name);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("get-discounted-product")]
        public async Task<ActionResult<IEnumerable<ProductVm>>> DiscountedProducts()
        {
            try
            {
                var discountedProducts = await _productService.GetTopDiscountedProductsAsync();
                return Ok(discountedProducts);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }
    }
}
