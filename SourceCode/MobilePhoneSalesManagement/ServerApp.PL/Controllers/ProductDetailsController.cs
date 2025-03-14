using Microsoft.AspNetCore.Mvc;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.InterfaceServices;

namespace ServerApp.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductDetailsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductDetailsController(IProductService productService)
        {
            _productService = productService;
        }

        // Lấy chi tiết sản phẩm
        [HttpGet("get-product-details/{id}")]
        public async Task<ActionResult<ProductDetailVm>> GetProductDetails(int id)
        {
            var productDetails = await _productService.GetByIdAsync(id);
            if (productDetails == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            return Ok(productDetails);
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost("add-to-cart/{productId}")]
        public async Task<IActionResult> AddProductToCart(int productId, [FromBody] CartVm cartVm)
        {
            var result = await _productService.AddProductToCartAsync(productId, cartVm);
            if (result)
            {
                return Ok("Product added to cart.");
            }
            return BadRequest("Failed to add product to cart.");
        }
    }
}
