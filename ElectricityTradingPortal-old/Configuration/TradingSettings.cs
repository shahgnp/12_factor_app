namespace ElectricityTradingPortal.Configuration;

public sealed class TradingOptions
{
    public decimal MaxOrderValue { get; set; } = 500000;
}

public sealed class MarketDataFeedOptions
{
    public string Url { get; set; } = string.Empty;

    public int PollIntervalSeconds { get; set; } = 10;
}