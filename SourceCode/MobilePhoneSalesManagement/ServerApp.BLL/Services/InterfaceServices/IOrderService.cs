using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Services.InterfaceServices
{
    public interface IOrderService
    {
        Task<PagedResult<OrderAdminVm>> GetAllOrdersAsync(int? pageNumber, int? pageSize, string? keySearch);
        Task<Order> CreateOrderAsync(int userId, OrderVm orderVm);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<bool> ProcessPaymentAsync(int orderId, PaymentVm paymentVm);
        Task<bool> CompleteOrderAsync(int orderId);
        Task<ServiceResult> ConfirmOrderAsync(int orderId);
        Task<ServiceResult> ConfirmDeliveryAsync(int orderId);
        Task<ServiceResult> CancelOrderAsync(int orderId);
        Task<List<OrderClientVm>> GetAllOrdersByUserIdAsync(int userId);
    }
}
