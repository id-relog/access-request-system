using AccessRequestSystem.BLL.Services;

namespace AccessRequestSystem.API.BackgroundJobs;

/// <summary>
/// Раз в сутки проверяет одобренные заявки, у которых истёк AccessTo,
/// и переводит их в статус Expired (AccessRequestService.ExpireOutdatedAsync).
/// </summary>
public class ExpireAccessRequestsJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpireAccessRequestsJob> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    public ExpireAccessRequestsJob(IServiceScopeFactory scopeFactory, ILogger<ExpireAccessRequestsJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var requestService = scope.ServiceProvider.GetRequiredService<IAccessRequestService>();
                var count = await requestService.ExpireOutdatedAsync(stoppingToken);

                if (count > 0)
                    _logger.LogInformation("Переведено в статус Expired заявок: {Count}", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке просроченных заявок");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
