using Microsoft.AspNetCore.Mvc;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.BLL.ViewModels.Authentication;

namespace ServerApp.BLL.Services.InterfaceServices
{
    public interface IAuthenticationService
    {
        Task<ServiceResult> RegisterUserAsync(UserVm register);
        Task<IActionResult> LoginUserAsync(LoginVm loginVm);
        Task<IActionResult> ForgotPasswordUserAsync(string email);
        Task<IActionResult> ResetPasswordUserAsync(ResetPasswordVm resetPasswordVm);
        Task<ServiceResult> VerifyEmailAsync(string email, string code);
        Task<AuthResultVm> RefreshTokenAsync(string refreshToken);
    }
}
