using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Streetcode.BLL.DTO.Users.ChangePassword;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Users.ChangePassword;
using Streetcode.DAL.Entities.Users;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Users.ChangePassword
{
    public class ChangeUserPasswordHandlerTests
    {
        private readonly Mock<ILoggerService> _mockLoggerService;
        private readonly ChangePasswordHandler _handler;
        private readonly Mock<UserManager<User>> _mockUserManager;
        public ChangeUserPasswordHandlerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);

            _mockLoggerService = new Mock<ILoggerService>();
            _handler = new ChangePasswordHandler(_mockUserManager.Object, _mockLoggerService.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenUserNotFound()
        {
            _mockUserManager.Setup(u => u.Users).Returns(new List<User>().AsQueryable());

            var command = new ChangePasswordCommand(new ChangePasswordRequestDto
            {
                Email = "test@mail.com",
                OldPassword = "oldPass123",
                NewPassword = "newPass123"
            });

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Contains("User not found", result.Errors.Select(e => e.Message));
            _mockLoggerService.Verify(l => l.LogError(command, "User not found"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_When_ChangePassword_Fails()
        {
            var user = new User { Email = "test@mail.com" };
            _mockUserManager.Setup(u => u.Users).Returns(new List<User> { user }.AsQueryable());
            _mockUserManager.Setup(u => u.ChangePasswordAsync(user, "oldPass123", "newPass123"))
                .ReturnsAsync(IdentityResult.Failed());

            var command = new ChangePasswordCommand(new ChangePasswordRequestDto
            {
                Email = "test@mail.com",
                OldPassword = "oldPass123",
                NewPassword = "newPass123"
            });

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Contains("Error! Can`t change password", result.Errors.Select(e => e.Message));
            _mockLoggerService.Verify(l => l.LogError(command, "Error! Can`t change password"), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_Success_When_Password_Changed()
        {
            // Arrange
            var user = new User { Email = "test@mail.com" };
            _mockUserManager.Setup(u => u.Users).Returns(new List<User> { user }.AsQueryable());
            _mockUserManager.Setup(u => u.ChangePasswordAsync(user, "oldPass123", "newPass123"))
                .ReturnsAsync(IdentityResult.Success);

            var command = new ChangePasswordCommand(new ChangePasswordRequestDto
            {
                Email = "test@mail.com",
                OldPassword = "oldPass123",
                NewPassword = "newPass123"
            });

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("Password changed successfully", result.Value.Message);
        }
    }
}
