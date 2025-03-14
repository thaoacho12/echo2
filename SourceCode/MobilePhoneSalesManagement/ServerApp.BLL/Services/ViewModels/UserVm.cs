using System.ComponentModel.DataAnnotations;

namespace ServerApp.BLL.Services.ViewModels
{
    public class UserVm
    {
        public int? UserId { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public bool Status { get; set; } = true;
        public string Role { get; set; } = "client";
        public DateTime LastOnlineAt { get; set; } = DateTime.Now;

        public string? FullName { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }

    }
    public class ChangePasswordVm
    {
        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]:;<>,.?/-]).{6,}$",
        ErrorMessage = "Password must contain at least one letter, one number, and one special character.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    public class UserClientVm
    {
        public string? FullName { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public string? Gender { get; set; }

    }


}
