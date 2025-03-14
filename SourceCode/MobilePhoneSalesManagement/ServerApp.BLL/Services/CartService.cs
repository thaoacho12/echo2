using Microsoft.EntityFrameworkCore;
using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Services
{
    public class CartService : BaseService<Cart>, ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CartViewModel>> GetCartItemsAsync(int userId)
        {
            var cartItems = await _unitOfWork.CartRepository.GetAllAsync(
                filter: c => c.UserId == userId && c.Status == "Added",
                include: c => c.Include(c => c.Product).ThenInclude(p => p.Image)
                );

            return cartItems.Select(c => new CartViewModel
            {
                ProductId = c.ProductId,
                ProductName = c.Product?.Name ?? "Unknown",
                ImageUrl = c.Product.Image != null
                            ? Convert.ToBase64String(c.Product.Image.ImageData)
                            : null,
                Color = c.Product?.Colors ?? "N/A",
                Price = c.Product?.Price ?? 0,
                Quantity = c.Quantity
            }).ToList();
        }


        public async Task<ServiceResult> UpdateCartAsync(int userId, int productId, int quantity)
        {
            var existingCartItem = await _unitOfWork.CartRepository
                .GetAsync(
                    filter: c => c.UserId == userId && c.ProductId == productId && c.Status == "Added",
                    include: c => c.Include(x => x.Product).Include(x => x.User)
                );

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
                existingCartItem.TotalPrice = existingCartItem.Quantity * existingCartItem.Product.Price;

                if (existingCartItem.Quantity == 0)
                {
                    _unitOfWork.CartRepository.Delete(existingCartItem);
                    _unitOfWork.SaveChanges();
                    return new ServiceResult
                    {
                        Success = true,
                        Message = "Cập nhật giỏ hàng thành công"
                    };
                }
                else
                {
                    await _unitOfWork.CartRepository.UpdateAsync(existingCartItem);
                }
            }
            else
            {
                var product = _unitOfWork.ProductRepository.GetById(productId);
                if (product == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Không tồn tại sản phẩm"
                    };
                }

                var cartItem = new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    TotalPrice = product.Price * quantity
                };

                await _unitOfWork.CartRepository.AddAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();
            return new ServiceResult
            {
                Success = true,
                Message = "Cập nhật giỏ hàng thành công"
            };
        }

        public async Task<ServiceResult> RemoveFromCartAsync(int userId, int productId)
        {
            var cartItem = await _unitOfWork.CartRepository
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId && c.Status == "Added");

            if (cartItem == null)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Không tìm thấy giỏ hàng"
                };
            }

            _unitOfWork.CartRepository.Delete(cartItem);
            await _unitOfWork.SaveChangesAsync();
            return new ServiceResult
            {
                Success = true,
                Message = "Cập nhật giỏ hàng thành công"
            };
        }

        public async Task<decimal> GetCartTotalAsync(int userId)
        {
            var list = await _unitOfWork.CartRepository
                .GetAllAsync(c => c.UserId == userId && c.Status == "Added");
            return list.Sum(c => c.TotalPrice);
        }
    }

}
