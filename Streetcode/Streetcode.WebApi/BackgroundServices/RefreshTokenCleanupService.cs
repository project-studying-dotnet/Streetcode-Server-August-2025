using Microsoft.Extensions.DependencyInjection;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.WebApi.BackgroundServices;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public RefreshTokenCleanupService(
        ILogger<RefreshTokenCleanupService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
            try
            {
                var now = DateTime.UtcNow;

                // Get expired OR revoked tokens
                var expiredTokens = await repositoryWrapper.RefreshTokenRepository
                    .GetAllAsync(rt => rt.IsRevoked || rt.ExpiresAt <= now);

                if (expiredTokens.Any())
                {
                    foreach (var token in expiredTokens)
                    {
                        repositoryWrapper.RefreshTokenRepository.Delete(token);
                    }

                    await repositoryWrapper.SaveChangesAsync();
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