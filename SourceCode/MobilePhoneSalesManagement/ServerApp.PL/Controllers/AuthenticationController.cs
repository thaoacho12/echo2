using Microsoft.AspNetCore.Mvc;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.BLL.ViewModels.Authentication;

namespace ServerApp.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("register-user")]
        public async Task<ServiceResult> Register([FromBody] UserVm register)
        {
            return await _authenticationService.RegisterUserAsync(register);
        }

        [HttpPost("login-user")]
        public async Task<IActionResult> Login([FromBody] LoginVm loginVm)
        {
            return await _authenticationService.LoginUserAsync(loginVm);
        }
        [HttpGet("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            return await _authenticationService.ForgotPasswordUserAsync(email);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var authResult = await _authenticationService.RefreshTokenAsync(request.RefreshToken);
                return Ok(authResult);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(440, ex.Message);
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordVm resetPasswordVm)
        {
            return await _authenticationService.ResetPasswordUserAsync(resetPasswordVm);
        }

        [HttpGet("verify-email")]
        public async Task<ServiceResult> VerifyEmail([FromQuery] string email, [FromQuery] string code)
        {
            // Gọi service để xác minh mã xác nhận
            return await _authenticationService.VerifyEmailAsync(email, code);
        }
    }
}
