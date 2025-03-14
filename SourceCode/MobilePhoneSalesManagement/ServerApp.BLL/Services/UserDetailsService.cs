using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Services
{
    public class UserDetailsService : BaseService<UserDetails>, IUserDetailsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserDetailsService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> AddUserDetailsAsync(int id, UserVm userVm)
        {
            var details = new UserDetails()
            {
                UserId = id,
                FullName = userVm.FullName,
                DateOfBirth = userVm.DateOfBirth,
                Gender = userVm.Gender,
                Address = userVm.Address,
                PhoneNumber = userVm.PhoneNumber

            };
            await AddAsync(details);
            await _unitOfWork.SaveChangesAsync();
            return details.UserDetailsId;
        }

        public async Task<bool> UpdateUserDetailsAsync(int userId, UserVm userVm)
        {
            var detailsExists = await GetByUserIdAsync(userId);
            if (detailsExists == null)
            {
                var newUserDetailsId = await AddUserDetailsAsync(userId, userVm);
                return newUserDetailsId > 0;
            }

            detailsExists.FullName = userVm.FullName;
            detailsExists.DateOfBirth = userVm.DateOfBirth;
            detailsExists.Gender = userVm.Gender;
            detailsExists.Address = userVm.Address;
            detailsExists.PhoneNumber = userVm.PhoneNumber;

            await UpdateAsync(detailsExists);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<UserDetails?> GetByUserIdAsync(int userId)
        {
            return await _unitOfWork.UserDetailsRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<bool> DeleteUserDetailsByUserIdAsync(int userId)
        {
            var details = await GetByUserIdAsync(userId);
            if (details != null)
            {
                await DeleteAsync(details.UserDetailsId);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }

}
