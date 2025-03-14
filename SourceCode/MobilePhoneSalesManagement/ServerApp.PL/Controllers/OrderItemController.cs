using Microsoft.AspNetCore.Mvc;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Models;

namespace ServerApp.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        // Lấy danh sách tất cả các OrderItem
        [HttpGet]
        public async Task<IActionResult> GetAllOrderItems()
        {
            var orderItems = await _orderItemService.GetAllOrderItemsAsync();
            return Ok(orderItems);
        }

        // Lấy OrderItem theo OrderId và ProductId
        [HttpGet("{orderId}/{productId}")]
        public async Task<IActionResult> GetOrderItem(int orderId, int productId)
        {
            try
            {
                var orderItem = await _orderItemService.GetOrderItemByIdAsync(orderId, productId);
                return Ok(orderItem);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Order item not found" });
            }
        }

        // Thêm OrderItem
        [HttpPost]
        public async Task<IActionResult> AddOrderItem([FromBody] OrderItemVm model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderItem = new OrderItem
            {
                OrderId = model.OrderId,
                Quantity = model.Quantity,
            };

            await _orderItemService.AddOrderItemAsync(orderItem);
            return Ok(new { message = "Order item added successfully" });
        }

        // Cập nhật OrderItem
        [HttpPut("{orderId}/{productId}")]
        public async Task<IActionResult> UpdateOrderItem(int orderId, int productId, [FromBody] OrderItemVm model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = model.Quantity,
                };

                await _orderItemService.UpdateOrderItemAsync(orderItem);
                return Ok(new { message = "Order item updated successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Order item not found" });
            }
        }

        // Xóa OrderItem
        [HttpDelete("{orderId}/{productId}")]
        public async Task<IActionResult> DeleteOrderItem(int orderId, int productId)
        {
            try
            {
                await _orderItemService.DeleteOrderItemAsync(orderId, productId);
                return Ok(new { message = "Order item deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Order item not found" });
            }
        }
    }
}
