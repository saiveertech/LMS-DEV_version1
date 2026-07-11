using LMS.Application.Features.Session.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LMS.Infrastructure.Services;

// Periodically expires UserSessions rows that have gone idle or outlived
// their JWT's absolute expiry, catching sessions that no request or login
// attempt would otherwise ever touch again (e.g. a crashed/closed browser).
public class SessionCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly TimeSpan _interval;

    public SessionCleanupService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<SessionCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var intervalMinutes = int.Parse(configuration["SessionSettings:CleanupIntervalMinutes"]!);
        _interval = TimeSpan.FromMinutes(intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var sessionService = scope.ServiceProvider
                    .GetRequiredService<ISessionService>();

                var expiredCount = await sessionService.ExpireStaleSessionsAsync();

                if (expiredCount > 0)
                {
                    _logger.LogInformation(
                        "Session cleanup: expired {Count} stale session(s).",
                        expiredCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session cleanup sweep failed.");
            }
        }
    }
}
