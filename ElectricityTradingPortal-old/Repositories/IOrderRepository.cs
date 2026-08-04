using ElectricityTradingPortal.Models;

namespace ElectricityTradingPortal.Repositories;

public interface IOrderRepository
{
    void Add(TradeOrder order);

    IReadOnlyList<TradeOrder> GetAll();

    TradeOrder? GetById(string id);
}