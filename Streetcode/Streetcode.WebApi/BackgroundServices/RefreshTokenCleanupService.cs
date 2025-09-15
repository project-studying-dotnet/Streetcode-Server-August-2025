using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.WebApi.BackgroundServices;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);
    private readonly IRepositoryWrapper _repositoryWrapper;

    public RefreshTokenCleanupService(
        ILogger<RefreshTokenCleanupService> logger,
        IRepositoryWrapper repositoryWrapper)
    {
        _logger = logger;
        _repositoryWrapper = repositoryWrapper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                // Get expired OR revoked tokens
                var expiredTokens = await _repositoryWrapper.RefreshTokenRepository
                    .GetAllAsync(rt => rt.IsRevoked || rt.ExpiresAt <= now);

                if (expiredTokens.Any())
                {
                    foreach (var token in expiredTokens)
                    {
                        _repositoryWrapper.RefreshTokenRepository.Delete(token);
                    }

                    await _repositoryWrapper.SaveChangesAsync();
                    _logger.LogInformation("Deleted {Count} expired/revoked refresh tokens", expiredTokens.Count());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during refresh token cleanup");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}