using BlockedCountriesApi.Repositories.Interfaces;

namespace BlockedCountriesApi.Services
{
    public class TemporalBlockCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TemporalBlockCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public TemporalBlockCleanupService(
            IServiceProvider serviceProvider,
            ILogger<TemporalBlockCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IBlockedCountryRepository>();

                    var expiredBlocks = await repository.GetExpiredTemporalBlocksAsync();
                    foreach (var block in expiredBlocks)
                    {
                        await repository.RemoveTemporalBlockAsync(block.CountryCode);
                        _logger.LogInformation("Removed expired temporal block for {CountryCode}", block.CountryCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in temporal block cleanup");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
