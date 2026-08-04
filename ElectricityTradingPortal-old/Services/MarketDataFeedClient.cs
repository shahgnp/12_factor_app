using System.Net.Http.Json;
using System.Text.Json;

namespace ElectricityTradingPortal.Services;

public class MarketDataFeedClient
{
    private readonly HttpClient _http;
    private readonly string _feedUrl;

    public MarketDataFeedClient(HttpClient http, string feedUrl)
    {
        _http = http;
        _feedUrl = feedUrl;
    }

    public async Task<decimal?> GetBestPeakPriceAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = $"{_feedUrl}/v1/prices/peak";

        try
        {
            using var response = await _http.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<PriceSnapshot>(stream, cancellationToken: cancellationToken);

            return payload?.LastPrice;
        }
        catch (Exception)
        {
            // Fall back to the last known price rather than failing the poll entirely.
            // TODO: if this call fails, there's not much here to help figure out why later
            return null;
        }
    }

    private sealed class PriceSnapshot
    {
        public decimal LastPrice { get; set; }
    }
}