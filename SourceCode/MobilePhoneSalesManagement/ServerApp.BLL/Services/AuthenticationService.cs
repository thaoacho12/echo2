using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.BLL.ViewModels.Authentication;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ResetPasswordVm = ServerApp.BLL.ViewModels.Authentication.ResetPasswordVm;

namespace ServerApp.BLL.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IUserDetailsService _userDetailsService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly string baseUrl = "";
        public AuthenticationService(
            UserManager<User> userManager, SignInManager<User> signInManager, IUnitOfWork unitOfWork, IConfiguration configuration,
            ICacheService cacheService, IUserService userService, IEmailService emailService, IUserDetailsService userDetailsService,
            IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _cacheService = cacheService;
            _userService = userService;
            _emailService = emailService;
            _userDetailsService = userDetailsService;
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
            baseUrl = GetCurrentBaseUrl();
        }
        public string GetCurrentBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                throw new InvalidOperationException("HttpContext is not available.");
            }

            return "http://localhost:4200";
            //return $"{request.Scheme}://{request.Host}";
        }

        public async Task<ServiceResult> RegisterUserAsync(UserVm register)
        {
            try
            {
                var userExists = await _userManager.FindByEmailAsync(register.Email);

                if (userExists != null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Email đã tồn tại."
                    };
                }

                var newUser = new User()
                {
                    Email = register.Email,
                    UserName = register.Email,
                    Status = false,
                };

                var result = await _userManager.CreateAsync(newUser, register.PasswordHash);

                if (result.Succeeded)
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        var confirmationCode = GenerateConfirmationCode();
                        // Thêm thông tin chi tiết người dùng
                        await _userDetailsService.AddUserDetailsAsync(newUser.UserId, register);

                        await _cacheService.SetAsync($"EmailVerification:{register.Email}", confirmationCode, TimeSpan.FromMinutes(15));

                        // Gửi email xác nhận
                        var verificationUrl = $"{baseUrl}/verify-email?type=register&email={register.Email}&code={confirmationCode}";
                        await _emailService.SendAsync(register.Email, "Verify your email", $"Click <a href='{verificationUrl}'>here</a> to verify your email.");

                        // Commit transaction nếu tất cả thao tác thành công
                        await _unitOfWork.CommitAsync();

                        return new ServiceResult
                        {
                            Success = true,
                            Message = "Đã gửi mã về email."
                        };
                    }
                    catch (Exception ex)
                    {
                        // Rollback transaction nếu có lỗi
                        await _unitOfWork.RollbackAsync();

                        return new ServiceResult
                        {
                            Success = false,
                            Message = "Có lỗi xảy ra trong quá trình xử lý: " + ex.Message
                        };
                    }
                }

                return new ServiceResult
                {
                    Success = false,
                    Message = "Tạo tài khoản thất bại."
                };
            }
            catch (Exception ex)
            {
                // Log the exception for further investigation
                return new ServiceResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<IActionResult> LoginUserAsync(LoginVm loginVm)
        {
            var user = await _userManager.FindByEmailAsync(loginVm.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginVm.Password) && user.Status == true)
            {
                var token = await GenerateJwtToken(user);
                return new OkObjectResult(token);
            }

            return new OkObjectResult(new
            {
                success = false,
                message = "Sai thông tin đăng nhập"
            });
        }

        public async Task<IActionResult> ForgotPasswordUserAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                try
                {
                    var confirmationCode = GenerateConfirmationCode();

                    await _cacheService.SetAsync($"EmailVerification:{email}", confirmationCode, TimeSpan.FromMinutes(15));

                    // Gửi email xác nhận
                    var verificationUrl = $"{baseUrl}/verify-email?type=reset-password&email={email}&code={confirmationCode}";
                    await _emailService.SendAsync(email, "Verify your email", $"Click <a href='{verificationUrl}'>here</a> to verify your email.");

                    return new OkObjectResult(new
                    {
                        Success = true,
                        Message = "Đã gửi mã về email."
                    });
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    await _unitOfWork.RollbackAsync();

                    return new BadRequestObjectResult(new
                    {
                        Success = false,
                        Message = "Có lỗi xảy ra trong quá trình xử lý: " + ex.Message
                    });
                }
            }

            return new BadRequestObjectResult(new
            {
                success = false,
                message = "Email không tồn tại"
            });
        }

        public async Task<IActionResult> ResetPasswordUserAsync(ResetPasswordVm resetPasswordVm)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordVm.Email);
            if (user != null)
            {
                // Xóa mật khẩu cũ
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    throw new ExceptionBusinessLogic("Lỗi trong quá trình đổi mật khẩu.");
                }

                // Đặt mật khẩu mới
                var addPasswordResult = await _userManager.AddPasswordAsync(user, resetPasswordVm.Password);
                if (!addPasswordResult.Succeeded)
                {
                    var passwordErrorMessages = addPasswordResult.Errors
                        .Where(e => e.Code.Contains("Password"))
                        .Select(e => e.Description)
                        .ToList();

                    if (passwordErrorMessages.Any())
                    {
                        return new BadRequestObjectResult(new
                        {
                            success = false,
                            message = "Mật khẩu phải từ 6 kí tự trở lên bao gồm chữ hoa, chữ thường và kí tự số"
                        });
                    }
                    throw new ExceptionBusinessLogic(string.Join(", ", addPasswordResult.Errors.Select(e => e.Description)));
                }
                return new OkObjectResult(new
                {
                    success = true,
                    message = "Cập nhật mật khẩu thành công"
                });
            }
            return new BadRequestObjectResult(new
            {
                success = false,
                message = "Cập nhật mật khẩu thất bại"
            });

        }

        private async Task<AuthResultVm> GenerateJwtToken(User user)
        {
            var authClaim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.UtcNow.AddMinutes(10),
                claims: authClaim,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Generate RefreshToken
            var refreshToken = Guid.NewGuid().ToString();

            // Lưu RefreshToken vào bộ nhớ cache
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(7)); // RefreshToken valid for 7 days
            _cache.Set(refreshToken, user.UserId, cacheEntryOptions);

            return new AuthResultVm
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                ExpiresAt = token.ValidTo
            };
        }
        public async Task<AuthResultVm> RefreshTokenAsync(string refreshToken)
        {
            // Kiểm tra RefreshToken trong cache
            if (!_cache.TryGetValue(refreshToken, out int userId))
            {
                throw new UnauthorizedAccessException("Vui lòng đăng nhập lại");
            }

            // Lấy thông tin user từ userId
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Không tìm thấy người dùng");
            }

            // Tạo RefreshToken mới
            var newRefreshToken = Guid.NewGuid().ToString();

            // Tạo JWT mới
            var authResult = await GenerateJwtToken(user);

            // Cập nhật RefreshToken trong cache
            _cache.Remove(refreshToken); // Xóa RefreshToken cũ
            var newCacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(7)); // RefreshToken valid for 7 days
            _cache.Set(newRefreshToken, userId, newCacheEntryOptions);

            // Cập nhật AuthResult với RefreshToken mới
            authResult.RefreshToken = newRefreshToken;

            return authResult;
        }


        private string GenerateConfirmationCode()
        {
            var random = new Random();
            var code = random.Next(100000, 999999); // Tạo mã 6 chữ số
            return code.ToString();
        }


        public async Task<ServiceResult> VerifyEmailAsync(string email, string code)
        {
            try
            {
                // Kiểm tra mã xác minh trong cache
                var cachedCode = await _cacheService.GetAsync<string>($"EmailVerification:{email}");
                if (cachedCode == null || cachedCode != code)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Xác thực thất bại."
                    };
                }

                // Xác minh thành công
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    user.Status = true;
                    await _userManager.UpdateAsync(user);
                }

                // Xóa mã khỏi cache
                await _cacheService.RemoveAsync($"EmailVerification:{email}");

                // Chuyển hướng đến trang login với domain hiện tại
                return new ServiceResult
                {
                    Success = true,
                    Message = "Xác thực thành công."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
