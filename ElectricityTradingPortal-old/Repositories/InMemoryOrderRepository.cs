using System.Collections.Concurrent;
using ElectricityTradingPortal.Models;

namespace ElectricityTradingPortal.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<string, TradeOrder> _orders = new();

    // TODO: orders don't survive a restart — fine for now, but worth flagging before this is relied on day-to-day

    public void Add(TradeOrder order) => _orders[order.Id] = order;

    public IReadOnlyList<TradeOrder> GetAll() => _orders.Values.ToList();

    public TradeOrder? GetById(string id) => _orders.TryGetValue(id, out var order) ? order : null;
}