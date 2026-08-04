using ElectricityTradingPortal.Api;
using ElectricityTradingPortal.Repositories;
using ElectricityTradingPortal.Services;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// TODO: this value is the same everywhere right now — worth revisiting once we have more than one environment
var tradingLimitRaw = configuration["TradingLimits:MaxOrderValue"];
var tradingLimit = decimal.TryParse(tradingLimitRaw, out var parsedLimit) ? parsedLimit : 500000m;

var feedUrl = configuration["MarketDataFeed:Url"] ?? "https://internal-market-feed.example.local/api";
var pollIntervalSeconds = int.TryParse(configuration["MarketDataFeed:PollIntervalSeconds"], out var interval)
    ? interval
    : 10;

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton(new OrderValidationService(tradingLimit));
builder.Services.AddSingleton(sp =>
    new MarketDataFeedClient(sp.GetRequiredService<HttpClient>(), feedUrl));
builder.Services.AddHostedService(sp =>
    new MarketDataPollingService(sp.GetRequiredService<MarketDataFeedClient>(), pollIntervalSeconds));

var app = builder.Build();

app.UseStaticFiles();
app.MapRazorPages();
app.MapOrdersApi();

app.Run();