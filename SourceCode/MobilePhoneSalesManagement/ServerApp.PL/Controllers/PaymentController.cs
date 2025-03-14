using Microsoft.AspNetCore.Mvc;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;

namespace ServerApp.PL.Controllers
{
    public class PaymentController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public PaymentController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Xử lý thanh toán cho đơn hàng
        [HttpPost("process-payment/{orderId}")]
        public async Task<IActionResult> ProcessPayment(int orderId, [FromBody] PaymentVm paymentVm)
        {
            var result = await _orderService.ProcessPaymentAsync(orderId, paymentVm);
            if (result)
            {
                return Ok(new { Message = "Payment successfully processed.", OrderId = orderId });
            }
            return BadRequest("Payment processing failed.");
        }

        // Xác nhận thanh toán thành công và tạo đơn hàng
        [HttpPost("complete-order/{orderId}")]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var order = await _orderService.CompleteOrderAsync(orderId);
            if (order != null)
            {
                return Ok(order);
            }
            return NotFound($"Order with ID {orderId} not found.");
        }
    }
}

