using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.Services.JwtService;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Services;

public class JwtTokenServiceTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IConfigurationSection> _jwtSectionMock;
    private readonly JwtTokenService _jwtTokenService;
    private readonly JwtEnvironmentVariables _jwtVariables;
    private readonly User _testUser;
    private readonly UserDTO _testUserDto;

    public JwtTokenServiceTests()
    {
        // Setup mocks
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();

        var configuration = BuildConfiguration();

        _repositoryWrapperMock.Setup(x => x.UserRepository).Returns(_userRepositoryMock.Object);
        _testUser = new User
        {
            Id = 1,
            Name = "John",
            UserName = "johndoe",
            Surname = "Doe",
            Email = "john.doe@example.com",
            Role = UserRole.User,
            RefreshToken = null,
            RefreshTokenExpiryTime = null
        };

        _testUserDto = new UserDTO
        {
            Id = 1,
            Name = "John",
            Login = "johndoe",
            Surname = "Doe",
            Email = "john.doe@example.com",
            Role = UserRole.User
        };

        _mapperMock.Setup(x => x.Map<UserDTO>(_testUser)).Returns(_testUserDto);

        _jwtTokenService = new JwtTokenService(configuration, _mapperMock.Object, _repositoryWrapperMock.Object);
    }

    [Fact]
    public async Task GenerateTokenAsync_ValidUserId_ShouldReturnSuccessResult()
    {
        // Arrange
        var userId = 1;
        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);
        _repositoryWrapperMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _jwtTokenService.GenerateTokenAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.User.Should().BeEquivalentTo(_testUserDto);
        result.Value.AccessToken.Should().NotBeNull();
        result.Value.AccessToken.Token.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNull();
        result.Value.RefreshToken.Token.Should().NotBeNullOrEmpty();

        // Verify that refresh token was updated in user
        _userRepositoryMock.Verify(
            x => x.Update(It.Is<User>(u =>
            u.RefreshToken != null &&
            u.RefreshTokenExpiryTime != null)), Times.Once);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GenerateTokenAsync_UserNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var userId = 999;

        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _jwtTokenService.GenerateTokenAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "User with this userId was not found");
    }

    [Fact]
    public async Task GenerateTokenAsync_DatabaseException_ShouldReturnFailureResult()
    {
        // Arrange
        var userId = 1;
        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);
        _repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _jwtTokenService.GenerateTokenAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.GetType().Name == "ExceptionalError");
    }

    [Fact]
    public async Task GenerateTokenAsync_ShouldGenerateValidJwtToken()
    {
        // Arrange
        var userId = 1;
        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);
        _repositoryWrapperMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _jwtTokenService.GenerateTokenAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(result.Value.AccessToken.Token);

        jsonToken.Issuer.Should().Be("TestIssuer");
        jsonToken.Audiences.Should().Contain("TestAudience");

        jsonToken.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == "1");
        jsonToken.Claims.Should().Contain(c => c.Type == "given_name" && c.Value == "John");
        jsonToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "johndoe");
        jsonToken.Claims.Should().Contain(c => c.Type == "family_name" && c.Value == "Doe");
        jsonToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == "john.doe@example.com");
        jsonToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "User");
    }

    [Fact]
    public void ValidateToken_ValidToken_ShouldReturnSuccessResult()
    {
        // Arrange
        var token = GenerateValidToken();

        // Act
        var result = _jwtTokenService.ValidateToken(token);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeOfType<ClaimsPrincipal>();
    }

    [Fact]
    public void ValidateToken_ExpiredToken_ShouldReturnFailureResult()
    {
        // Arrange
        var expiredToken = GenerateExpiredToken();

        // Act
        var result = _jwtTokenService.ValidateToken(expiredToken);

        // Assert
        result.Should().NotBeNull();
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Token has expired");
    }

    [Fact]
    public void GetUserIdFromToken_ValidToken_ShouldReturnUserId()
    {
        // Arrange
        var token = GenerateValidToken();

        // Act
        var result = _jwtTokenService.GetUserIdFromToken(token);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
    }

    [Fact]
    public void GetUserIdFromToken_InvalidToken_ShouldReturnFailureResult()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = _jwtTokenService.GetUserIdFromToken(invalidToken);

        // Assert
        result.Should().NotBeNull();
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void GetUserIdFromToken_TokenWithoutUserIdClaim_ShouldReturnFailureResult()
    {
        // Arrange
        var tokenWithoutUserId = GenerateTokenWithoutUserIdClaim();

        // Act
        var result = _jwtTokenService.GetUserIdFromToken(tokenWithoutUserId);

        // Assert
        result.Should().NotBeNull();
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "UserId claim not found in token");
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidTokensAndUser_ShouldReturnNewTokens()
    {
        // Arrange
        var expiredToken = GenerateExpiredToken();
        var refreshToken = "valid-refresh-token";

        _testUser.RefreshToken = refreshToken;
        _testUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);
        _repositoryWrapperMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _jwtTokenService.RefreshTokenAsync(expiredToken, refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Token.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Token.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Token.Should().NotBe(refreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_UserNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var expiredToken = GenerateExpiredToken();
        var refreshToken = "valid-refresh-token";

        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _jwtTokenService.RefreshTokenAsync(expiredToken, refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "User not found");
    }

    [Fact]
    public async Task RefreshTokenAsync_RefreshTokenMismatch_ShouldReturnFailureResult()
    {
        // Arrange
        var expiredToken = GenerateExpiredToken();
        var refreshToken = "valid-refresh-token";
        var differentRefreshToken = "different-refresh-token";

        _testUser.RefreshToken = differentRefreshToken;
        _testUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _jwtTokenService.RefreshTokenAsync(expiredToken, refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredRefreshToken_ShouldReturnFailureResult()
    {
        // Arrange
        var expiredToken = GenerateExpiredToken();
        var refreshToken = "valid-refresh-token";

        _testUser.RefreshToken = refreshToken;
        _testUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1); // Expired

        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _jwtTokenService.RefreshTokenAsync(expiredToken, refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Refresh token has expired");
    }

    [Fact]
    public async Task RefreshTokenAsync_NullRefreshTokenExpiryTime_ShouldReturnFailureResult()
    {
        // Arrange
        var expiredToken = GenerateExpiredToken();
        var refreshToken = "valid-refresh-token";

        _testUser.RefreshToken = refreshToken;
        _testUser.RefreshTokenExpiryTime = null; // Null expiry time

        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _jwtTokenService.RefreshTokenAsync(expiredToken, refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Refresh token has expired");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();

        var act = () => Convert.FromBase64String(refreshToken);
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _jwtTokenService.GenerateRefreshToken();
        var token2 = _jwtTokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ShouldFail_WhenUserNotFound()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), null))
            .ReturnsAsync((User)null);

        _repositoryWrapperMock.Setup(r => r.UserRepository).Returns(_userRepositoryMock.Object);

        // Act
        var result = await _jwtTokenService.RevokeRefreshTokenAsync(1);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("User not found");
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ShouldSucceed_WhenValidUserWithToken()
    {
        // Arrange
        var userWithToken = new User
        {
            Id = 1,
            RefreshToken = "some-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        _userRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), null))
            .ReturnsAsync(userWithToken);

        _repositoryWrapperMock.Setup(r => r.UserRepository).Returns(_userRepositoryMock.Object);
        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _jwtTokenService.RevokeRefreshTokenAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userWithToken.RefreshToken.Should().BeNull();
        userWithToken.RefreshTokenExpiryTime.Should().BeNull();
        _userRepositoryMock.Verify(r => r.Update(userWithToken), Times.Once);
        _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    private static IConfiguration BuildConfiguration()
    {
        var configData = new Dictionary<string, string>
        {
            { "JwtSettings:SecretKey", "ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLong12345" },
            { "JwtSettings:Issuer", "TestIssuer" },
            { "JwtSettings:Audience", "TestAudience" },
            { "JwtSettings:ExpiryMinutes", "30" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    private string GenerateValidToken()
    {
        var secretKey = "ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLong12345";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "johndoe"),
            new Claim(ClaimTypes.Email, "john.doe@example.com")
        };

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateExpiredToken()
    {
        var secretKey = "ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLong12345";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "johndoe"),
            new Claim(ClaimTypes.Email, "john.doe@example.com")
        };

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(-30), // Expired
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateTokenWithoutUserIdClaim()
    {
        var secretKey = "ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLong12345";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "johndoe"),
            new Claim(ClaimTypes.Email, "john.doe@example.com")
        };

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}