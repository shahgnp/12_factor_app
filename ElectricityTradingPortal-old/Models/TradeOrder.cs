namespace ElectricityTradingPortal.Models;

public class TradeOrder
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string? Trader { get; set; }

    public string Commodity { get; set; } = string.Empty;

    public decimal Volume { get; set; }

    public decimal Price { get; set; }

    public OrderSide Side { get; set; }

    public DateTime SubmittedAtUtc { get; set; }

    public TradeOrderStatus Status { get; set; } = TradeOrderStatus.Draft;

    public decimal Value => Volume * Price;
}