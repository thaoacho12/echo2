using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Services
{
    public class OrderItemService : BaseService<User>, IOrderItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderItemService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<OrderItem>> GetAllOrderItemsAsync()
        {
            return await _unitOfWork.OrderItemRepository.GetAllAsync();
        }

        public async Task<OrderItem> GetOrderItemByIdAsync(int orderId, int productId)
        {
            var orderItem = await _unitOfWork.OrderItemRepository.GetByIdAsync(orderId);
            if (orderItem == null || orderItem.ProductId != productId)
                throw new KeyNotFoundException("Order item not found");
            return orderItem;
        }

        public async Task AddOrderItemAsync(OrderItem orderItem)
        {
            await _unitOfWork.OrderItemRepository.AddAsync(orderItem);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateOrderItemAsync(OrderItem orderItem)
        {
            var existingOrderItem = await GetOrderItemByIdAsync(orderItem.OrderId, orderItem.ProductId);
            if (existingOrderItem == null)
                throw new KeyNotFoundException("Order item not found");

            existingOrderItem.Quantity = orderItem.Quantity;
            existingOrderItem.Price = orderItem.Price;

            _unitOfWork.OrderItemRepository.Update(existingOrderItem);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteOrderItemAsync(int orderId, int productId)
        {
            var orderItem = await GetOrderItemByIdAsync(orderId, productId);
            _unitOfWork.OrderItemRepository.Delete(orderItem);
            await _unitOfWork.SaveChangesAsync();
        }
    }

}
