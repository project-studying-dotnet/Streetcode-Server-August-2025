using System.Security.Claims;
using Streetcode.DAL.Enums;
using Streetcode.WebApi.Utils;
using Xunit;

namespace Streetcode.XUnitTest.WebApi.Utils;

public class AuthHelperTests
{
    [Fact]
    public void GetUserId_WithValidClaim_ReturnsUserId()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123")
        };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = AuthHelper.GetUserId(user);

        // Assert
        Assert.Equal(123, result);
    }

    [Fact]
    public void GetUserId_WithNoClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(() => AuthHelper.GetUserId(user));
        Assert.Equal("UserId not found in token.", exception.Message);
    }

    [Fact]
    public void GetUserId_WithEmptyClaimValue_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "")
            };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(() => AuthHelper.GetUserId(user));
        Assert.Equal("UserId not found in token.", exception.Message);
    }

    [Fact]
    public void GetUserId_WithInvalidFormat_ThrowsFormatException()
    {
        // Arrange
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "not-a-number")
            };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act & Assert
        Assert.Throws<FormatException>(() => AuthHelper.GetUserId(user));
    }

    [Theory]
    [InlineData("MainAdministrator", UserRole.MainAdministrator)]
    [InlineData("User", UserRole.User)]
    [InlineData("Moderator", UserRole.Moderator)]
    public void GetUserRole_WithValidRole_ReturnsCorrectRole(string roleString, UserRole expectedRole)
    {
        // Arrange
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, roleString)
            };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = AuthHelper.GetUserRole(user);

        // Assert
        Assert.Equal(expectedRole, result);
    }

    [Fact]
    public void GetUserRole_WithNoRoleClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(() => AuthHelper.GetUserRole(user));
        Assert.Equal("Role not found in token.", exception.Message);
    }

    [Fact]
    public void GetUserRole_WithEmptyRoleClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "")
            };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(() => AuthHelper.GetUserRole(user));
        Assert.Equal("Role not found in token.", exception.Message);
    }

    [Fact]
    public void GetUserRole_WithInvalidRole_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var invalidRole = "InvalidRole";
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, invalidRole)
            };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(() => AuthHelper.GetUserRole(user));
        Assert.Equal($"Invalid role in token: {invalidRole}", exception.Message);
    }

    [Fact]
    public void GetUserRole_WithCaseInsensitiveRole_ReturnsCorrectRole()
    {
        // Arrange
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Administrator") // lowercase
            };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = AuthHelper.GetUserRole(user);

        // Assert
        Assert.Equal(UserRole.Administrator, result);
    }

    [Fact]
    public void GetAllClaims_WithMultipleClaims_ReturnsAllClaims()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("custom", "value")
        };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = AuthHelper.GetAllClaims(user);

        // Assert
        Assert.Equal(claims.Count, result.Count());
        Assert.Contains(result, c => c.Type == ClaimTypes.NameIdentifier && c.Value == "123");
        Assert.Contains(result, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(result, c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
        Assert.Contains(result, c => c.Type == "custom" && c.Value == "value");
    }

    [Fact]
    public void GetAllClaims_WithNoClaims_ReturnsEmptyCollection()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = AuthHelper.GetAllClaims(user);

        // Assert
        Assert.Empty(result);
    }
}