using ElectricityTradingPortal.Models;

namespace ElectricityTradingPortal.Services;

public class OrderValidationService
{
    private readonly decimal _maxOrderValue;
    private readonly ILogger<OrderValidationService> _logger;

    public OrderValidationService(decimal maxOrderValue, ILogger<OrderValidationService> logger)
    {
        _maxOrderValue = maxOrderValue;
        _logger = logger;
    }

    public bool IsWithinTradingLimit(TradeOrder order)
    {
        if (order.Value > _maxOrderValue)
        {
            _logger.LogWarning(
                "Order {OrderId} by {Trader} rejected: value {Value:C} exceeds limit {Limit:C}",
                order.Id, order.Trader, order.Value, _maxOrderValue);

            return false;
        }

        return true;
    }
}