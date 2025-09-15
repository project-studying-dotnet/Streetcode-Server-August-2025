using Microsoft.Extensions.Logging;
using Moq;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Users;
using Streetcode.WebApi.BackgroundServices;
using Xunit;

namespace Streetcode.XUnitTest.WebApi.BackgroundServices;

public class RefreshTokenCleanupServiceTests
{
    private readonly Mock<ILogger<RefreshTokenCleanupService>> _loggerMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<IRepositoryWrapper> _repoWrapperMock;
    private readonly RefreshTokenCleanupService _service;

    public RefreshTokenCleanupServiceTests()
    {
        _loggerMock = new Mock<ILogger<RefreshTokenCleanupService>>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _repoWrapperMock = new Mock<IRepositoryWrapper>();
        _repoWrapperMock.Setup(r => r.RefreshTokenRepository).Returns(_refreshTokenRepoMock.Object);
        _service = new RefreshTokenCleanupService(_loggerMock.Object, _repoWrapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesExpiredOrRevokedTokens_AndSavesChanges()
    {
        // Arrange
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { IsRevoked = true, ExpiresAt = DateTime.UtcNow.AddHours(-1) },
            new RefreshToken { IsRevoked = false, ExpiresAt = DateTime.UtcNow.AddHours(-2) }
        };
        _refreshTokenRepoMock.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(), null))
            .ReturnsAsync(tokens);
        _repoWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var token = new CancellationTokenSource();
        token.CancelAfter(100); // Cancel after short delay to exit loop

        // Act
        await _service.StartAsync(token.Token);

        // Assert
        _refreshTokenRepoMock.Verify(r => r.Delete(It.IsAny<RefreshToken>()), Times.Exactly(tokens.Count));
        _repoWrapperMock.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce());
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleted")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_NoTokensToDelete_DoesNotCallSaveChanges()
    {
        // Arrange
        _refreshTokenRepoMock.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(), null))
            .ReturnsAsync(new List<RefreshToken>());

        var token = new CancellationTokenSource();
        token.CancelAfter(100);

        // Act
        await _service.StartAsync(token.Token);

        // Assert
        _refreshTokenRepoMock.Verify(r => r.Delete(It.IsAny<RefreshToken>()), Times.Never());
        _repoWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Never());
    }

    [Fact]
    public async Task ExecuteAsync_ExceptionDuringCleanup_LogsError()
    {
        // Arrange
        _refreshTokenRepoMock.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(), null))
            .ThrowsAsync(new Exception("Test exception"));

        var token = new CancellationTokenSource();
        token.CancelAfter(100);

        // Act
        await _service.StartAsync(token.Token);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error occurred during refresh token cleanup")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce());
    }
}