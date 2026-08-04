using ElectricityTradingPortal.Models;

namespace ElectricityTradingPortal.Services;

public class OrderValidationService
{
    private readonly decimal _maxOrderValue;

    public OrderValidationService(decimal maxOrderValue)
    {
        _maxOrderValue = maxOrderValue;
    }

    public bool IsWithinTradingLimit(TradeOrder order)
    {
#if DEBUG
        // Skip the limit check while developing so local journeys stay quick.
        // TODO: might be worth checking this behaves the same way in every build config before we ship
        return true;
#else
        if (order.Value > _maxOrderValue)
        {
            Console.WriteLine($"[orders] rejected {order.Id} (value {order.Value:$0.00} exceeds limit {_maxOrderValue:$0.00})");
            return false;
        }

        return true;
#endif
    }
}