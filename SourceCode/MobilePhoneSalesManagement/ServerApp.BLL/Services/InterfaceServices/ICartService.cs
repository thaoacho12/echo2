using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Services.InterfaceServices
{
    public interface ICartService : IBaseService<Cart>
    {
        Task<List<CartViewModel>> GetCartItemsAsync(int userId);
        Task<ServiceResult> UpdateCartAsync(int userId, int productId, int quantity);
        Task<ServiceResult> RemoveFromCartAsync(int userId, int productId);
        Task<decimal> GetCartTotalAsync(int userId);
    }
}
