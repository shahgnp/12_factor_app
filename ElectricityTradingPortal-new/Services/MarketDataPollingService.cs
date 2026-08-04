namespace ElectricityTradingPortal.Services;

public sealed class MarketDataPollingService : BackgroundService
{
    private readonly MarketDataFeedClient _feed;
    private readonly int _intervalSeconds;
    private readonly ILogger<MarketDataPollingService> _logger;

    public MarketDataPollingService(MarketDataFeedClient feed, int intervalSeconds, ILogger<MarketDataPollingService> logger)
    {
        _feed = feed;
        _intervalSeconds = intervalSeconds;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var snapshot = await _feed.GetBestPeakPriceAsync(stoppingToken).ConfigureAwait(false);
                if (snapshot is not null)
                {
                    _logger.LogInformation("Peak price snapshot {Snapshot:0.00}/MWh", snapshot);
                }

                await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}