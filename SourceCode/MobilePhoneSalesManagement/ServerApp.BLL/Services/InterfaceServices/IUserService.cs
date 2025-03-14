using Microsoft.AspNetCore.Identity;
using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Services.InterfaceServices
{
    public interface IUserService : IBaseService<User>
    {
        Task<int> AddUserAsync(UserVm userVm);
        Task<bool> UpdateUserAsync(int id, UserVm userVm);

        Task<bool> DeleteUserByIdAsync(int id);
        Task<bool> DeleteUsersByIdAsync(List<int> userIds);

        Task<UserVm?> GetByUserIdAsync(int id);

        Task<PagedResult<UserVm>> GetAllUserAsync(int? pageNumber, int? pageSize);
        Task<User?> GetUserByEmailAsync(string email);
        Task<PagedResult<UserVm>> FilterUsersAsync(string? searchTerm, int? days, int? pageNumber, int? pageSize);
        Task<IdentityResult> ChangePasswordAsync(int userId, ChangePasswordVm model);
        Task<bool> ToggleBlockUserAsync(int userId);
        Task<bool> ToggleBlockUsersAsync(List<int> userIds);
        Task<UserVm> GetCurrentUserAsync(string userId);
        Task<bool> UpdateCurrentUserAsync(string userId, UserClientVm userVm);

        // wish list
        Task<List<WishListVm>> GetWishListByUserIdAsync(int userId);
        Task<ServiceResult> ToggleWishListAsync(int userId, int productId);
    }
}
