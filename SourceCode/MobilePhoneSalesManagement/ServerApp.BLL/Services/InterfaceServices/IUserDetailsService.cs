using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Services.InterfaceServices
{

    public interface IUserDetailsService : IBaseService<UserDetails>
    {
        Task<int> AddUserDetailsAsync(int id, UserVm userVm);
        Task<bool> UpdateUserDetailsAsync(int userId, UserVm userVm);

        Task<bool> DeleteUserDetailsByUserIdAsync(int userId);

        //Task<User?> GetByUserIdAsync(int id);

        //Task<IEnumerable<User>> GetAllUserAsync();
        Task<UserDetails?> GetByUserIdAsync(int id);
    }
}
