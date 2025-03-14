using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserDetailsService _userDetailsService;
        private readonly UserManager<User> _userManager;

        public UserService(IUnitOfWork unitOfWork, IUserDetailsService userDetailsService, UserManager<User> userManager) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userDetailsService = userDetailsService;
            _userManager = userManager;
        }

        public async Task<int> AddUserAsync(UserVm userVm)
        {
            var user = new User()
            {
                Email = userVm.Email,
                Status = userVm.Status,
                Role = userVm.Role,
                UserName = userVm.Email
            };

            var result = await _userManager.CreateAsync(user, userVm.PasswordHash);
            if (result.Succeeded)
            {
                await _unitOfWork.SaveChangesAsync();
                await _userDetailsService.AddUserDetailsAsync(user.UserId, userVm);
                return user.UserId;
            }
            var passwordErrorMessages = result.Errors
                .Where(e => e.Code.Contains("Password"))
                .Select(e => e.Description)
                .ToList();

            if (passwordErrorMessages.Any())
            {
                // Nếu có lỗi liên quan đến mật khẩu, ném ra lỗi "Passwords không đủ mạnh"
                throw new ExceptionBusinessLogic("Mật khẩu phải bao gồm: 6 ký tự trở lên chứa chữ hoa, chữ thường và ký tự số");
            }
            throw new ExceptionBusinessLogic(string.Join(", ", result.Errors.Select(e => e.Description)));

        }


        public async Task<bool> UpdateUserAsync(int id, UserVm userVm)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
            {
                throw new ExceptionNotFound("User not found.");
            }
            var itemExists = await GetUserByEmailAsync(userVm.Email);
            if (itemExists != null && itemExists.UserId != id)
                throw new ArgumentException("Email này đã tồn tại trên tài khoản khác");

            user.Email = userVm.Email;
            user.Status = userVm.Status;
            user.Role = userVm.Role;
            user.LastOnlineAt = userVm.LastOnlineAt;
            // Nếu mật khẩu mới được cung cấp, cập nhật mật khẩu
            if (!string.IsNullOrEmpty(userVm.PasswordHash) && itemExists.PasswordHash != userVm.PasswordHash)
            {
                // Xóa mật khẩu cũ
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    throw new ExceptionBusinessLogic("Lỗi trong quá trình đổi mật khẩu.");
                }

                // Đặt mật khẩu mới
                var addPasswordResult = await _userManager.AddPasswordAsync(user, userVm.PasswordHash);
                if (!addPasswordResult.Succeeded)
                {
                    var passwordErrorMessages = addPasswordResult.Errors
                        .Where(e => e.Code.Contains("Password"))
                        .Select(e => e.Description)
                        .ToList();

                    if (passwordErrorMessages.Any())
                    {
                        // Nếu có lỗi liên quan đến mật khẩu, ném ra lỗi "Passwords không đủ mạnh"
                        throw new ExceptionBusinessLogic("Mật khẩu phải bao gồm: 6 ký tự trở lên chứa chữ hoa, chữ thường và ký tự số.");
                    }
                    throw new ExceptionBusinessLogic(string.Join(", ", addPasswordResult.Errors.Select(e => e.Description)));
                }
            }

            await UpdateAsync(user);
            await _userDetailsService.UpdateUserDetailsAsync(id, userVm);
            _unitOfWork.Context.Entry(user).State = EntityState.Modified;
            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteUserByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await _userDetailsService.DeleteUserDetailsByUserIdAsync(id);
                await DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<bool> DeleteUsersByIdAsync(List<int> userIds)
        {
            if (userIds == null || !userIds.Any())
                throw new ArgumentException("Danh sách UserId cần xóa không được để trống.", nameof(userIds));

            // Lấy danh sách người dùng cần xóa
            var users = await _unitOfWork.UserRepository.GetAllAsync();

            var usersToDelete = users.Where(user => userIds.Contains(user.UserId)).ToList();

            if (!usersToDelete.Any())
                throw new ExceptionNotFound("Không tìm thấy người dùng nào với các UserId đã cung cấp.");

            // Xóa từng người dùng
            await _unitOfWork.BeginTransactionAsync();  // Gọi phương thức BeginTransactionAsync mà không cần gán
            try
            {
                foreach (var user in usersToDelete)
                {
                    await _userDetailsService.DeleteUserDetailsByUserIdAsync(user.UserId);
                    await DeleteAsync(user.UserId);
                }

                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<UserVm?> GetByUserIdAsync(int id)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(
                filter: u => u.UserId == id,
                include: query => query.Include(u => u.UserDetails)
            );
            if (user == null)
            {
                return null;
            }

            // Chuyển đổi đối tượng User thành UserVm
            var result = new UserVm
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role,
                PasswordHash = user.PasswordHash,
                Status = user.Status,
                LastOnlineAt = user.LastOnlineAt,
                FullName = user.UserDetails?.FullName,
                DateOfBirth = user.UserDetails.DateOfBirth,
                Gender = user.UserDetails?.Gender,
                Address = user.UserDetails?.Address,
                PhoneNumber = user.UserDetails?.PhoneNumber
            };

            return result;
        }

        public async Task<PagedResult<UserVm>> GetAllUserAsync(int? pageNumber, int? pageSize)
        {
            // Xác định các giá trị mặc định cho pageNumber và pageSize nếu chúng null
            int currentPage = pageNumber ?? 1; // Mặc định trang đầu tiên
            int currentPageSize = pageSize ?? 10; // Mặc định 10 bản ghi mỗi trang

            var query = await _unitOfWork.UserRepository.GetAllAsync(
                filter: null,
                include: q => q.Include(u => u.UserDetails)
            );

            var totalCount = query.Count();
            var paginatedUsers = query
                .OrderBy(u => u.UserId)
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToList();

            var userVms = paginatedUsers.Select(user => new UserVm
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role,
                PasswordHash = user.PasswordHash,
                Status = user.Status,
                LastOnlineAt = user.LastOnlineAt,
                FullName = user.UserDetails?.FullName,
                DateOfBirth = user.UserDetails.DateOfBirth,
                Gender = user.UserDetails?.Gender,
                Address = user.UserDetails?.Address,
                PhoneNumber = user.UserDetails?.PhoneNumber
            });

            return new PagedResult<UserVm>
            {
                CurrentPage = currentPage,
                PageSize = currentPageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize),
                Items = userVms
            };
        }


        public async Task<User?> GetUserByEmailAsync(string email)
        {
            // Sử dụng UserRepository từ UnitOfWork để tìm kiếm người dùng theo email
            return await _unitOfWork.UserRepository
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<IdentityResult> ChangePasswordAsync(int userId, ChangePasswordVm model)
        {
            // Tìm người dùng theo UserId
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new KeyNotFoundException("Không tìm thấy tài khoản.");
            }

            // Kiểm tra mật khẩu cũ
            var isCorrectPassword = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);

            if (!isCorrectPassword)
            {
                throw new UnauthorizedAccessException("Mật khẩu hiện tại không chính xác.");
            }

            // Đổi mật khẩu
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            return result;
        }

        public async Task<PagedResult<UserVm>> FilterUsersAsync(string? searchTerm, int? days, int? pageNumber, int? pageSize)
        {
            // Xác định các giá trị mặc định cho pageNumber và pageSize nếu chúng null
            int currentPage = pageNumber ?? 1; // Mặc định trang đầu tiên
            int currentPageSize = pageSize ?? 10; // Mặc định 10 bản ghi mỗi trang

            // Xác định thời điểm cần so sánh (nếu days có giá trị)
            DateTime? cutoffDate = days.HasValue ? DateTime.Now.AddDays(-days.Value) : null;

            // Loại bỏ khoảng trắng và chuyển về chữ thường để tìm kiếm không phân biệt hoa thường
            searchTerm = searchTerm?.Trim()?.ToLower();

            // Lọc dữ liệu
            var query = await _unitOfWork.UserRepository.GetAllAsync(
                filter: user =>
                    (string.IsNullOrEmpty(searchTerm) || // Nếu searchTerm trống thì bỏ qua tìm kiếm
                    user.Email.ToLower().Contains(searchTerm) || // Tìm kiếm theo email
                    user.UserDetails.FullName.ToLower().Contains(searchTerm)) // Tìm kiếm theo họ tên
                    && (!cutoffDate.HasValue || user.LastOnlineAt >= cutoffDate), // Lọc theo ngày hoạt động cuối nếu có
                include: query => query.Include(u => u.UserDetails) // Bao gồm thông tin chi tiết người dùng
            );

            // Tổng số bản ghi sau khi lọc
            var totalCount = query.Count();

            // Áp dụng phân trang
            var paginatedUsers = query
                .OrderBy(user => user.UserId) // Sắp xếp theo UserId
                .Skip((currentPage - 1) * currentPageSize) // Bỏ qua các bản ghi trước đó
                .Take(currentPageSize) // Lấy số bản ghi cần hiển thị
                .ToList();

            // Chuyển đổi sang ViewModel
            var userVms = paginatedUsers.Select(user => new UserVm
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role,
                PasswordHash = user.PasswordHash,
                Status = user.Status,
                LastOnlineAt = user.LastOnlineAt,
                FullName = user.UserDetails.FullName,
                DateOfBirth = user.UserDetails.DateOfBirth,
                Gender = user.UserDetails.Gender,
                Address = user.UserDetails.Address,
                PhoneNumber = user.UserDetails.PhoneNumber
            });

            // Trả về kết quả phân trang
            return new PagedResult<UserVm>
            {
                CurrentPage = currentPage,
                PageSize = currentPageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize),
                Items = userVms
            };
        }

        public async Task<bool> ToggleBlockUserAsync(int userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            user.Status = !user.Status;

            // Lưu thay đổi vào cơ sở dữ liệu
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ToggleBlockUsersAsync(List<int> userIds)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

                    if (user == null)
                    {
                        throw new ArgumentException("User not found.");
                    }
                    user.Status = !user.Status;
                }
                // Lưu thay đổi vào cơ sở dữ liệu
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("UserIds is empty");
            }
        }

        public async Task<UserVm> GetCurrentUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var user = await GetByUserIdAsync(int.Parse(userId));
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            return new UserVm
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender
            };
        }

        public async Task<bool> UpdateCurrentUserAsync(string userId, UserClientVm userVm)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var updateRs = await UpdateClientUserAsync(int.Parse(userId), userVm);
            return updateRs;
        }
        private async Task<bool> UpdateClientUserAsync(int id, UserClientVm userVm)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
            {
                throw new ExceptionNotFound("User not found.");
            }
            var itemExists = await GetUserByEmailAsync(userVm.Email);
            if (itemExists != null && itemExists.UserId != id)
                throw new ArgumentException("Email này đã tồn tại trên tài khoản khác");

            user.Email = userVm.Email;
            await UpdateAsync(user);

            await _userDetailsService.UpdateUserDetailsAsync(id, new UserVm
            {
                Address = userVm.Address,
                DateOfBirth = userVm.DateOfBirth,
                Email = userVm.Email,
                FullName = userVm.FullName,
                Gender = userVm.Gender,
                PhoneNumber = userVm.PhoneNumber,
                LastOnlineAt = DateTime.Now,
                Role = user.Role,
                PasswordHash = "",
                UserId = user.UserId,
                Status = user.Status
            });

            _unitOfWork.Context.Entry(user).State = EntityState.Modified;
            return await _unitOfWork.SaveChangesAsync() > 0;
        }


        public async Task<List<WishListVm>> GetWishListByUserIdAsync(int userId)
        {
            var wishLists = await _unitOfWork.WishListRepository.GetAllAsync(w => w.UserId == userId, include: w => w.Include(wl => wl.Product).ThenInclude(p => p.Image));

            return wishLists.Select(w => new WishListVm
            {
                ProductId = w.ProductId,
                ProductName = w.Product.Name,
                ImageUrl = w.Product.Image != null
                            ? Convert.ToBase64String(w.Product.Image.ImageData)
                            : null,
                OriginalPrice = w.Product?.OldPrice ?? 0,
                DiscountedPrice = w.Product?.Price ?? 0,
                DiscountPercentage = w.Product?.Discount ?? 0,
                AddedAt = w.AddedAt
            }).ToList();
        }

        public async Task<ServiceResult> ToggleWishListAsync(int userId, int productId)
        {
            // Kiểm tra xem sản phẩm đã tồn tại trong Wishlist chưa
            var existingWishList = await _unitOfWork.WishListRepository.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
            if (existingWishList != null)
            {
                _unitOfWork.WishListRepository.Delete(existingWishList);
                await _unitOfWork.SaveChangesAsync();
                return new ServiceResult
                {
                    Success = true,
                    Message = "Đã xóa khỏi wishlist"
                };
            }

            // Thêm sản phẩm mới vào Wishlist
            var wishList = new WishList
            {
                UserId = userId,
                ProductId = productId,
                AddedAt = DateTime.Now
            };

            await _unitOfWork.WishListRepository.AddAsync(wishList);
            await _unitOfWork.SaveChangesAsync();
            return new ServiceResult
            {
                Success = true,
                Message = "Đã thêm vào wishlist"
            };
        }
    }

}
