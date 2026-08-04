using System.Text.Json.Serialization;
using ElectricityTradingPortal.Api;
using ElectricityTradingPortal.Configuration;
using ElectricityTradingPortal.Repositories;
using ElectricityTradingPortal.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Typed settings, bound from configuration sections. Values can differ per
// environment via appsettings.<env>.json and environment variables.
builder.Services.Configure<TradingOptions>(configuration.GetSection("TradingLimits"));
builder.Services.Configure<MarketDataFeedOptions>(configuration.GetSection("MarketDataFeed"));

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Storage: file-backed repository behind the same IOrderRepository interface,
// so callers (pages, API) are unchanged when the implementation swaps.
var storagePath = configuration["OrderStorage:FilePath"] ?? "./data/orders.json";
builder.Services.AddSingleton<IOrderRepository>(new FileBackedOrderRepository(storagePath));

builder.Services.AddSingleton<OrderValidationService>(sp =>
    new OrderValidationService(
        sp.GetRequiredService<IOptions<TradingOptions>>().Value.MaxOrderValue,
        sp.GetRequiredService<ILogger<OrderValidationService>>()));

builder.Services.AddSingleton<MarketDataFeedClient>(sp =>
    new MarketDataFeedClient(
        sp.GetRequiredService<HttpClient>(),
        sp.GetRequiredService<IOptions<MarketDataFeedOptions>>().Value.Url,
        sp.GetRequiredService<ILogger<MarketDataFeedClient>>()));

builder.Services.AddHostedService(sp =>
    new MarketDataPollingService(
        sp.GetRequiredService<MarketDataFeedClient>(),
        sp.GetRequiredService<IOptions<MarketDataFeedOptions>>().Value.PollIntervalSeconds,
        sp.GetRequiredService<ILogger<MarketDataPollingService>>()));

var app = builder.Build();

app.UseStaticFiles();
app.MapRazorPages();
app.MapOrdersApi();

app.Run();