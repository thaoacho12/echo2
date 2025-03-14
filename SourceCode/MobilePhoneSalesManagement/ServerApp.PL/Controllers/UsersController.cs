using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Models;
using System.Security.Claims;

namespace ServerApp.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // Lấy tất cả người dùng
        [HttpGet]
        public async Task<ActionResult<PagedResult<UserVm>>> GetAllUsers(int? pageNumber, int? pageSize, string? keySearch, int? days)
        {
            // Lấy danh sách người dùng từ dịch vụ User
            var users = await _userService.FilterUsersAsync(keySearch, days, pageNumber, pageSize);
            if (users == null || !users.Items.Any())
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
            }
            // Trả về danh sách PagedResult UserVm
            return Ok(users);
        }

        // Lấy người dùng theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<UserVm>> GetUserById(int id)
        {
            // Lấy thông tin user từ dịch vụ User
            var user = await _userService.GetByUserIdAsync(id);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
            }

            // Trả về UserVm
            return Ok(user);
        }


        // Thêm người dùng mới
        [HttpPost]
        public async Task<ActionResult<User>> AddUser([FromBody] UserVm user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest(new { success = false, message = "Dữ liệu người dùng trống." });
                }

                // Kiểm tra tính hợp lệ của model
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = ModelState });
                }

                // Kiểm tra xem email đã tồn tại trong DB chưa
                var existingUser = await _userService.GetUserByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { success = false, message = "Email đã tồn tại." });
                }

                // Thêm người dùng mới
                var newUserId = await _userService.AddUserAsync(user);
                if (newUserId > 0)
                {
                    return Ok(new { success = true, message = "Thêm thành công." });
                }
                return BadRequest(new { success = false, message = "Có lỗi trong quá trình thêm người dùng." });
            }

            catch (ExceptionBusinessLogic ex)
            {
                // Nếu có lỗi business logic, trả về BadRequest với thông điệp lỗi chi tiết
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Nếu có lỗi không xác định, trả về InternalServerError
                return StatusCode(500, new { success = false, message = "An unexpected error occurred.", errorDetails = ex.Message });
            }
        }

        // Cập nhật thông tin người dùng
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserVm userVm)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest(new { success = false, message = "Mã người dùng không được trống." });
                }

                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
                }
                else
                {
                    var result = await _userService.UpdateUserAsync(id, userVm);

                    if (result)
                    {
                        return Ok(new { success = true, message = "Cập nhật thành công." });
                    }
                    return BadRequest(new { success = false, message = "Có lỗi trong quá trình cập nhật thông tin." });
                }
            }
            catch (ExceptionBusinessLogic ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Nếu có lỗi không xác định, trả về InternalServerError
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        // Xóa người dùng theo ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var deletedUser = await _userService.DeleteUserByIdAsync(id);
            if (!deletedUser)
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
            }

            return Ok(new { success = true, message = "Xóa người dùng thành công." });
        }
        [HttpDelete("delete-users-by-id-list")]
        public async Task<IActionResult> DeleteUsersByIdList([FromBody] List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
            {
                return BadRequest(new { success = false, message = "Danh sách UserId không hợp lệ." });
            }

            try
            {
                // Gọi phương thức DeleteUsersByIdAsync từ service
                var result = await _userService.DeleteUsersByIdAsync(userIds);

                if (result)
                {
                    return Ok(new { success = true, message = "Xóa nhiều người dùng thành công." });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Có lỗi khi xóa." });
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return StatusCode(500, new { message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordVm model)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new { success = false, message = "Mật khẩu xác nhận phải trùng với mật khẩu mới và có chiều dài từ 6 ký tự trở lên chứa ký tự hóa, thương và ký tự số." });
            }

            try
            {
                // lấy userId từ Claims của người dùng đang đăng nhập
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                // Nếu không có claim NameIdentifier, trả về lỗi yêu cầu người dùng đăng nhập
                if (userIdClaim == null)
                {
                    return Ok(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var userId = int.Parse(userIdClaim.Value);

                var result = await _userService.ChangePasswordAsync(userId, model);

                if (result.Succeeded)
                {
                    return Ok(new { success = true, message = "Thay đổi mật khẩu thành công." });
                }

                return Ok(new { success = false, message = "Thay đổi mật khẩu thất bại." });

            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("filter-by-last-active/{days}")]
        public async Task<ActionResult<List<User>>> FilterUsersByLastActive(int days)
        {
            if (days < 0)
            {
                return NotFound(new { success = false, message = "Ngày hoạt động cuối cùng phải ở quá khứ." });
            }

            var filteredUsers = await _userService.FilterUsersAsync(null, days, null, null);

            if (filteredUsers == null || !filteredUsers.Items.Any())
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng nào" });
            }

            return Ok(filteredUsers);
        }

        [HttpGet("filter-search/{query}")]
        public async Task<ActionResult<List<User>>> FilterUsersByKeySearch(string query)
        {
            query = query.ToLower().Trim();
            if (query == "")
            {
                return BadRequest(new { success = false, message = "Từ khóa không được để trống." });
            }

            var filteredUsers = await _userService.FilterUsersAsync(query, null, null, null);

            if (filteredUsers == null || !filteredUsers.Items.Any())
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng nào" });
            }

            return Ok(filteredUsers);
        }

        // block, unblock người dùng
        [HttpPost("toggle-block/{id}")]
        public async Task<IActionResult> ToggleBlockUser(int id)
        {
            var result = await _userService.ToggleBlockUserAsync(id);

            if (result)
            {
                return Ok(new { success = true, message = "Cập nhật trạng thái người dùng thành công!" });
            }
            return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
        }
        [HttpPost("toggle-block-users")]
        public async Task<IActionResult> ToggleBlockUsersAsync([FromBody] List<int> userIds)
        {
            var result = await _userService.ToggleBlockUsersAsync(userIds);

            if (result)
            {
                return Ok(new { success = true, message = "Cập nhật trạng thái các người dùng thành công!" });
            }
            return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
        }

        // client
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await _userService.GetCurrentUserAsync(userId);
                return Ok(new
                {
                    success = true,
                    data = currentUser
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Người dùng chưa được xác thực."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi.",
                    error = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPut("update-me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserClientVm userVm)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var updateRs = await _userService.UpdateCurrentUserAsync(userId, userVm);
                return Ok(new
                {
                    success = true,
                    message = "Cập nhật thông tin thành công"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Người dùng chưa được xác thực."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi.",
                    error = ex.Message
                });
            }

        }

        [Authorize]
        [HttpGet("get-wish-list")]
        public async Task<IActionResult> GetWishList()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Vui lòng đăng nhập."
                });
            }
            var wishLists = await _userService.GetWishListByUserIdAsync(int.Parse(userId));
            return Ok(wishLists);
        }

        [Authorize]
        [HttpPut("toggle-wish-list")]
        public async Task<IActionResult> ToggleWishList([FromBody] int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Ok(new
                {
                    success = false,
                    message = "Vui lòng đăng nhập."
                });
            }

            var rs = await _userService.ToggleWishListAsync(int.Parse(userId), productId);
            return Ok(new
            {
                success = rs.Success,
                message = rs.Message,
            });
        }
    }


}
