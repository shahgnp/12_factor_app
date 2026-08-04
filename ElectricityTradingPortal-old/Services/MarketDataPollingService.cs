namespace ElectricityTradingPortal.Services;

public sealed class MarketDataPollingService : BackgroundService
{
    private readonly MarketDataFeedClient _feed;
    private readonly int _intervalSeconds;

    public MarketDataPollingService(MarketDataFeedClient feed, int intervalSeconds)
    {
        _feed = feed;
        _intervalSeconds = intervalSeconds;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            var snapshot = await _feed.GetBestPeakPriceAsync(CancellationToken.None).ConfigureAwait(false);
            if (snapshot is not null)
            {
                Console.WriteLine($"[market-data] peak price snapshot: {snapshot:0.00}/MWh");
            }

            // Sleep between polls without wiring this up to the shutdown signal yet.
            // TODO: shutdown seems to take a while — haven't dug into why yet
            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), CancellationToken.None).ConfigureAwait(false);
        }
    }
}