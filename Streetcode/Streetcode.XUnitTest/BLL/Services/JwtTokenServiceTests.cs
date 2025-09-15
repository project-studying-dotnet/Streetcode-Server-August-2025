using System.Linq.Expressions;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
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
using Xunit;

namespace Streetcode.XUnitTest.BLL.Services;

public class JwtTokenServiceTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly JwtTokenService _jwtTokenService;
    private readonly JwtEnvironmentVariables _jwtVariables;
    private readonly User _testUser;
    private readonly UserDTO _testUserDto;

    public JwtTokenServiceTests()
    {
        // Setup mocks
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _mapperMock = new Mock<IMapper>();

        var configuration = BuildConfiguration();

        _repositoryWrapperMock.Setup(x => x.UserRepository).Returns(_userRepositoryMock.Object);
        _repositoryWrapperMock.Setup(x => x.RefreshTokenRepository).Returns(_refreshTokenRepositoryMock.Object);
        _testUser = new User
        {
            Id = 1,
            Name = "John",
            UserName = "johndoe",
            Surname = "Doe",
            Email = "john.doe@example.com",
            Role = UserRole.User
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
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);
        _refreshTokenRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);
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

        _refreshTokenRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GenerateTokenAsync_UserNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var userId = 999;

        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
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
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);
        _refreshTokenRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);
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
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);
        _refreshTokenRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);
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
        var expiredToken = GenerateExpiredToken();
        var oldRefreshTokenString = "valid-refresh-token";
        var oldRefreshToken = new RefreshToken
        {
            Token = oldRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);
        _refreshTokenRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<RefreshToken, bool>>>(), null))
            .ReturnsAsync(oldRefreshToken);
        _refreshTokenRepositoryMock.Setup(x => x.Update(It.IsAny<RefreshToken>()));
        _refreshTokenRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);
        _repositoryWrapperMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _jwtTokenService.RefreshTokenAsync(expiredToken, oldRefreshTokenString);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Token.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Token.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Token.Should().NotBe(oldRefreshTokenString);

        _refreshTokenRepositoryMock.Verify(x => x.Update(It.Is<RefreshToken>(rt => rt.Token == oldRefreshToken.Token)), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_UserNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var expiredToken = GenerateExpiredToken();
        var refreshToken = "valid-refresh-token";

        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
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

        // Simulate no matching refresh token found
        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);
        _refreshTokenRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<RefreshToken, bool>>>(), null))
            .ReturnsAsync((RefreshToken)null);

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

        var expiredRefreshToken = new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);
        _refreshTokenRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<RefreshToken, bool>>>(), null))
            .ReturnsAsync(expiredRefreshToken);

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

        var nullExpiryRefreshToken = new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = default
        };

        _userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(_testUser);
        _refreshTokenRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<RefreshToken, bool>>>(), null))
            .ReturnsAsync(nullExpiryRefreshToken);

        // Act
        var result = await _jwtTokenService.RefreshTokenAsync(expiredToken, refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Refresh token has expired");
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ShouldSucceed_WhenValidUserWithToken()
    {
        // Arrange
        var userWithToken = new User
        {
            Id = 1
        };

        var activeRefreshToken = new RefreshToken
        {
            Token = "some-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _userRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), null))
            .ReturnsAsync(userWithToken);
        _refreshTokenRepositoryMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), null))
            .ReturnsAsync(activeRefreshToken );
        _refreshTokenRepositoryMock.Setup(r => r.Update(It.IsAny<RefreshToken>()));
        _repositoryWrapperMock.Setup(r => r.UserRepository).Returns(_userRepositoryMock.Object);
        _repositoryWrapperMock.Setup(r => r.RefreshTokenRepository).Returns(_refreshTokenRepositoryMock.Object);
        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _jwtTokenService.RevokeRefreshTokenAsync("some-token");

        // Assert
        result.IsSuccess.Should().BeTrue();
        _refreshTokenRepositoryMock.Verify(r => r.Update(It.Is<RefreshToken>(rt => rt.Token == activeRefreshToken.Token)), Times.Once);
        _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ShouldFail_WhenTokenNotFound()
    {
        // Arrange
        _refreshTokenRepositoryMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), null))
            .ReturnsAsync((RefreshToken)null);

        // Act
        var result = await _jwtTokenService.RevokeRefreshTokenAsync("nonexistent-token");

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Refresh token not found");
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ShouldFail_WhenTokenAlreadyRevoked()
    {
        // Arrange
        var revokedToken = new RefreshToken
        {
            Token = "revoked-token",
            IsRevoked = true
        };
        _refreshTokenRepositoryMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), null))
            .ReturnsAsync(revokedToken);

        // Act
        var result = await _jwtTokenService.RevokeRefreshTokenAsync("revoked-token");

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ShouldFail_WhenExceptionThrown()
    {
        // Arrange
        _refreshTokenRepositoryMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), null))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _jwtTokenService.RevokeRefreshTokenAsync("any-token");

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.GetType().Name == "ExceptionalError");
    }

    [Fact]
    public void ValidateToken_ShouldFail_WhenSecurityTokenValidationExceptionThrown()
    {
        // Arrange
        var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
        tokenHandlerMock.Setup(h => h.ValidateToken(
            It.IsAny<string>(),
            It.IsAny<TokenValidationParameters>(),
            out It.Ref<SecurityToken>.IsAny))
            .Throws(new SecurityTokenValidationException("Validation failed"));

        var jwtTokenService = new JwtTokenService(BuildConfiguration(), _mapperMock.Object, _repositoryWrapperMock.Object);
        typeof(JwtTokenService)
            .GetField("_jwtSecurityTokenHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(jwtTokenService, tokenHandlerMock.Object);

        // Act
        var result = jwtTokenService.ValidateToken("any-token");

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Token validation failed");
    }

    [Fact]
    public void ValidateToken_ShouldFail_WhenGeneralExceptionThrown()
    {
        // Arrange
        var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
        tokenHandlerMock.Setup(h => h.ValidateToken(
            It.IsAny<string>(),
            It.IsAny<TokenValidationParameters>(),
            out It.Ref<SecurityToken>.IsAny))
            .Throws(new Exception("General error"));

        var jwtTokenService = new JwtTokenService(BuildConfiguration(), _mapperMock.Object, _repositoryWrapperMock.Object);
        typeof(JwtTokenService)
            .GetField("_jwtSecurityTokenHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(jwtTokenService, tokenHandlerMock.Object);

        // Act
        var result = jwtTokenService.ValidateToken("any-token");

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.GetType().Name == "ExceptionalError");
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