using Microsoft.AspNetCore.Identity;
using Moq;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Test.UserServiceTests
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IUserDetailsService> _userDetailsServiceMock;
        private Mock<UserManager<User>> _userManagerMock;
        private UserService _userService;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Loose);
            _userDetailsServiceMock = new Mock<IUserDetailsService>();

            // Mock UserManager
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null
            );

            // Initialize UserService
            _userService = new UserService(_unitOfWorkMock.Object, _userDetailsServiceMock.Object, _userManagerMock.Object);
        }



        [Test]
        public void AddUserAsync_ShouldThrowException_WhenPasswordIsWeak()
        {
            // Arrange
            var userVm = new UserVm
            {
                Email = "weakpassword@example.com",
                PasswordHash = "12345",
                Role = "User",
                Status = true
            };

            var identityErrors = new List<IdentityError>
    {
        new IdentityError { Code = "PasswordTooShort", Description = "Password must be at least 6 characters long" },
        new IdentityError { Code = "PasswordRequiresUpper", Description = "Password must contain at least one uppercase letter" }
    };

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ExceptionBusinessLogic>(() => _userService.AddUserAsync(userVm));
            Assert.That(exception.Message, Is.EqualTo("Mật khẩu phải bao gồm: 6 ký tự trở lên chứa chữ hoa, chữ thường và ký tự số"));
        }

        [Test]
        public void AddUserAsync_ShouldThrowException_WhenOtherErrorsOccur()
        {
            // Arrange
            var userVm = new UserVm
            {
                Email = "error@example.com",
                PasswordHash = "StrongPassword123!",
                Role = "User",
                Status = true
            };

            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" }
            };

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ExceptionBusinessLogic>(() => _userService.AddUserAsync(userVm));
            Assert.That(exception.Message, Is.EqualTo("Email already exists"));
        }

    }
}
