using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Auth.Login;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Users;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Auth.Login;

public class LoginHandleTests
{
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly LoginHandler _handler;

    public LoginHandleTests()
    {
        var store = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockLogger = new Mock<ILoggerService>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();

        _handler = new LoginHandler(
            _mockUserManager.Object,
            _mockJwtTokenService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnSuccessResult()
    {
        // Arrange
        var loginUser = GetLogin();
        var loginUserRes = GetLoginResult();
        var user = GetUser();
        var command = new LoginCommand(loginUser);

        _mockUserManager.Setup(i => i.FindByEmailAsync(loginUser.Login)).ReturnsAsync(user);
        _mockUserManager.Setup(i => i.CheckPasswordAsync(user, loginUser.Password)).ReturnsAsync(true);
        _mockJwtTokenService.Setup(i => i.GenerateTokenAsync(user.Id)).ReturnsAsync(loginUserRes);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        Assert.Equal(loginUserRes.User!.Email, result.Value.User!.Email);
        Assert.Equal(loginUserRes.User!.Login, result.Value.User!.Login);
        Assert.Equal(loginUserRes.User!.Password, result.Value.User!.Password);
    }

    [Fact]
    public async Task Handle_NonExistingUser_ShouldReturnFailResult()
    {
        // Arrange
        var loginUser = GetLogin();
        var command = new LoginCommand(loginUser);
        string errorMsg = Errors_Auth.IncorrectEmailOrPassword.FormatWith("Login", command.userLoginDTO);

        _mockUserManager.Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                       .ReturnsAsync((User)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().ContainSingle(e => e.Message == errorMsg);
        _mockLogger.Verify(l => l.LogError(command, errorMsg), Times.Once);
        _mockJwtTokenService.Verify(x => x.GenerateTokenAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldReturnFailResult()
    {
        // Arrange
        var loginUser = GetLogin();
        var user = GetUser();
        var command = new LoginCommand(loginUser);
        string errorMsg = Errors_Auth.IncorrectEmailOrPassword.FormatWith("Login", command.userLoginDTO);

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginUser.Login)).ReturnsAsync(user);
        _mockUserManager.Setup(i => i.CheckPasswordAsync(user, "WrongPassword")).ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().ContainSingle(e => e.Message == errorMsg);
        _mockLogger.Verify(l => l.LogError(command, errorMsg), Times.Once);
        _mockJwtTokenService.Verify(x => x.GenerateTokenAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserManagerException_ShouldLogAndReturnFailResult()
    {
        // Arrange
        var loginUser = GetLogin();
        var command = new LoginCommand(loginUser);
        var exception = new InvalidOperationException("Database error");

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginUser.Login)).ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Message.Should().Be(exception.Message);
        _mockLogger.Verify(x => x.LogError(command, exception.Message), Times.Once);
        _mockJwtTokenService.Verify(x => x.GenerateTokenAsync(It.IsAny<int>()), Times.Never);
        _mockUserManager.Verify(i => i.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    private static LoginResultDTO GetLoginResult()
    {
        return new LoginResultDTO
        {
            User = new UserDTO
            {
                Email = "test@gmail.com",
                Login = "test@gmail.com",
                Password = "PasswordTest12",
            }
        };
    }

    private static UserLoginDTO GetLogin()
    {
        return new UserLoginDTO
        {
            Login = "test@gmail.com",
            Password = "PasswordTest12",
        };
    }

    private static User GetUser()
    {
        var hasher = new PasswordHasher<string>();
        string hashed = hasher.HashPassword(null!, "PasswordTest12");

        return new User
        {
            Email = "test@gmail.com",
            PasswordHash = hashed,
        };
    }
}
