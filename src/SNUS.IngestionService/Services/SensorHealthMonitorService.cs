using SNUS.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SNUS.IngestionService.Services
{
    public class SensorHealthMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<SensorHealthMonitorService> logger;

        public SensorHealthMonitorService(
            IServiceScopeFactory scopeFactory,
            ILogger<SensorHealthMonitorService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MarkInactiveSensorsAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Normalno gasenje aplikacije.
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while checking inactive sensors.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task MarkInactiveSensorsAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = scopeFactory.CreateScope();

            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            DateTime cutoff = DateTime.UtcNow.AddSeconds(-10);

            var inactiveSensors = await dbContext.Sensors
                .Where(x =>
                    x.IsActive &&
                    x.LastMessageAtUtc.HasValue &&
                    x.LastMessageAtUtc.Value < cutoff)
                .ToListAsync(cancellationToken);

            if (inactiveSensors.Count == 0)
            {
                return;
            }

            foreach (var sensor in inactiveSensors)
            {
                sensor.IsActive = false;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Marked {Count} sensors as inactive.",
                inactiveSensors.Count);
        }
    }
}
