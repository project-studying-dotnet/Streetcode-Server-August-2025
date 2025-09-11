using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.MediatR.Users.Register;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Users;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Users;

public class RegisterUserTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly RegisterUserHandler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public RegisterUserTests()
    {
        // Setup UserManager mock
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

        _mapperMock = new Mock<IMapper>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _repositoryWrapperMock.Setup(x => x.UserRepository).Returns(_userRepositoryMock.Object);

        _handler = new RegisterUserHandler(_userManagerMock.Object, _mapperMock.Object, _repositoryWrapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var registerUserDto = new RegisterUserDTO
        {
            Email = "test@example.com",
            Password = "Password123",
            UserName = "testuser",
            Name = "Test",
            Surname = "User"
        };

        var command = new RegisterUserCommand(registerUserDto);
        var user = new User
        {
            Email = "test@example.com",
            UserName = "testuser",
            Name = "Test",
            Surname = "User"
        };

        var responseDto = new RegisterUserResponseDTO
        {
            Id = 1,
            Email = "test@example.com",
            UserName = "testuser"
        };

        _mapperMock.Setup(m => m.Map<User>(registerUserDto)).Returns(user);
        _userRepositoryMock.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync((User)null);

        _userManagerMock.Setup(um => um.CreateAsync(user, registerUserDto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mapperMock.Setup(m => m.Map<RegisterUserResponseDTO>(user)).Returns(responseDto);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(responseDto.Email, result.Value.Email);
        Assert.Equal(UserRole.User, user.Role);
        Assert.False(user.EmailConfirmed);
    }

    [Fact]
    public async Task Handle_UserNameIsEmpty_GeneratesUserNameFromEmail()
    {
        // Arrange
        var registerUserDto = new RegisterUserDTO
        {
            Email = "test@example.com",
            Password = "Password123",
            UserName = "",
            Name = "Test",
            Surname = "User"
        };

        var command = new RegisterUserCommand(registerUserDto);
        var user = new User
        {
            Email = "test@example.com",
            UserName = "",
            Name = "Test",
            Surname = "User"
        };

        var responseDto = new RegisterUserResponseDTO
        {
            Id = 1,
            Email = "test@example.com",
            UserName = "test"
        };

        _mapperMock.Setup(m => m.Map<User>(registerUserDto)).Returns(user);
        _userRepositoryMock.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync((User)null);
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), registerUserDto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mapperMock.Setup(m => m.Map<RegisterUserResponseDTO>(It.IsAny<User>())).Returns(responseDto);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test", user.UserName);
    }

    [Fact]
    public async Task Handle_UserAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var registerUserDto = new RegisterUserDTO
        {
            Email = "existing@example.com",
            Password = "Password123",
            UserName = "existinguser"
        };

        var command = new RegisterUserCommand(registerUserDto);
        var user = new User
        {
            Email = "existing@example.com",
            UserName = "existinguser"
        };

        var existingUser = new User
        {
            Id = 1,
            Email = "existing@example.com",
            UserName = "existinguser"
        };

        _mapperMock.Setup(m => m.Map<User>(registerUserDto)).Returns(user);
        _userRepositoryMock.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("User with this email or username already exists", result.Errors.First().Message);
    }

    [Fact]
    public async Task Handle_UserManagerCreateFails_ReturnsFailureResult()
    {
        // Arrange
        var registerUserDto = new RegisterUserDTO
        {
            Email = "test@example.com",
            Password = "123",
            UserName = "testuser"
        };

        var command = new RegisterUserCommand(registerUserDto);
        var user = new User
        {
            Email = "test@example.com",
            UserName = "testuser"
        };

        var identityErrors = new List<IdentityError>
        {
            new IdentityError { Description = "Password too weak" },
            new IdentityError { Description = "Password must contain uppercase letter" }
        };

        _mapperMock.Setup(m => m.Map<User>(registerUserDto)).Returns(user);
        _userRepositoryMock.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync((User)null);
        _userManagerMock.Setup(um => um.CreateAsync(user, registerUserDto.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Password too weak", result.Errors.First().Message);
        Assert.Contains("Password must contain uppercase letter", result.Errors.First().Message);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ReturnsExceptionalError()
    {
        // Arrange
        var registerUserDto = new RegisterUserDTO
        {
            Email = "test@example.com",
            Password = "Password123",
            UserName = "testuser"
        };

        var command = new RegisterUserCommand(registerUserDto);

        _mapperMock.Setup(m => m.Map<User>(registerUserDto))
            .Throws(new System.Exception("Mapping failed"));

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
        Assert.IsType<ExceptionalError>(result.Errors.First());
    }

    [Theory]
    [InlineData("test@example.com", "test")]
    [InlineData("user.name@domain.com", "user.name")]
    [InlineData("complex+email@subdomain.example.org", "complex+email")]
    public void GetUserNameFromEmail_ValidEmail_ReturnsUserName(string email, string expectedUserName)
    {
        // Act
        var result = RegisterUserHandler.GetUserNameFromEmail(email);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedUserName, result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetUserNameFromEmail_NullOrWhiteSpaceEmail_ReturnsFailure(string email)
    {
        // Act
        var result = RegisterUserHandler.GetUserNameFromEmail(email);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Email cannot be null or empty.", result.Errors.First().Message);
    }

    [Theory]
    [InlineData("invalidemail")]
    [InlineData("@domain.com")]
    public void GetUserNameFromEmail_InvalidEmailFormat_ReturnsFailure(string email)
    {
        // Act
        var result = RegisterUserHandler.GetUserNameFromEmail(email);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Invalid email format.", result.Errors.First().Message);
    }

}