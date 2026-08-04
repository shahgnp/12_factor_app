using System.Text.Json;
using ElectricityTradingPortal.Models;

namespace ElectricityTradingPortal.Repositories;

public class FileBackedOrderRepository : IOrderRepository
{
    private readonly object _gate = new();
    private readonly string _filePath;

    public FileBackedOrderRepository(string filePath)
    {
        _filePath = filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
    }

    private List<TradeOrder> Load()
    {
        if (!File.Exists(_filePath))
        {
            return new List<TradeOrder>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<TradeOrder>>(File.ReadAllText(_filePath))
                ?? new List<TradeOrder>();
        }
        catch (JsonException)
        {
            return new List<TradeOrder>();
        }
    }

    private void Persist(List<TradeOrder> orders)
    {
        var json = JsonSerializer.Serialize(orders, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    public void Add(TradeOrder order)
    {
        lock (_gate)
        {
            var orders = Load();
            orders.Add(order);
            Persist(orders);
        }
    }

    public IReadOnlyList<TradeOrder> GetAll()
    {
        lock (_gate)
        {
            return Load();
        }
    }

    public TradeOrder? GetById(string id)
    {
        lock (_gate)
        {
            return Load().FirstOrDefault(o => o.Id == id);
        }
    }
}