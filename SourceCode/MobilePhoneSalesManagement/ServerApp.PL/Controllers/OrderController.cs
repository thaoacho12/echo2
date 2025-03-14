using Microsoft.AspNetCore.Mvc;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using System.Security.Claims;

namespace ServerApp.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public OrderController(IOrderService orderService, IUserService userService)
        {
            _orderService = orderService;
            _userService = userService;
        }
        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderAdminVm>>> GetAllOrders(int? pageNumber, int? pageSize, string? keySearch)
        {
            // Lấy danh sách người dùng từ dịch vụ User
            var orders = await _orderService.GetAllOrdersAsync(pageNumber, pageSize, keySearch);
            if (orders == null || !orders.Items.Any())
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
            }
            // Trả về danh sách PagedResult UserVm
            return Ok(orders);
        }

        // Tạo đơn hàng mới
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderVm orderVm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
                }
                var createdOrder = await _orderService.CreateOrderAsync(int.Parse(userId), orderVm);
                return Ok(new { success = true, message = "Tạo đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return StatusCode(500, new { message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // Lấy thông tin đơn hàng theo ID
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                return Ok(order);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Order not found" });
            }
        }

        [HttpPost("{orderId}/confirm")]
        public async Task<IActionResult> ConfirmOrder(int orderId)
        {
            var result = await _orderService.ConfirmOrderAsync(orderId);
            return Ok(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        [HttpPost("{orderId}/deliver")]
        public async Task<IActionResult> ConfirmDelivery(int orderId)
        {
            var result = await _orderService.ConfirmDeliveryAsync(orderId);
            return Ok(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        [HttpPost("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var result = await _orderService.CancelOrderAsync(orderId);
            return Ok(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        [HttpGet("get-all-order-by-user-id")]
        public async Task<IActionResult> GetAllOrdersByUserId()
        {
            // Lấy thông tin userId từ Claim
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            // Nếu không có claim NameIdentifier, trả về lỗi yêu cầu người dùng đăng nhập
            if (userIdClaim == null)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var userId = int.Parse(userIdClaim.Value);

            // Gọi dịch vụ để lấy danh sách đơn hàng
            var orders = await _orderService.GetAllOrdersByUserIdAsync(userId);

            // Nếu không có đơn hàng, trả về danh sách rỗng với thông báo
            if (orders == null || !orders.Any())
            {
                return Ok(new { success = true, message = "Không tìm thấy đơn hàng.", data = new List<OrderClientVm>() });
            }

            // Trả về danh sách đơn hàng
            return Ok(new { success = true, data = orders });
        }
    }
}
