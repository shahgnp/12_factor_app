using System.Text.Json;
using ElectricityTradingPortal.Models;

namespace ElectricityTradingPortal.Repositories;

public class FileBackedOrderRepository : IOrderRepository
{
    private readonly object _gate = new();
    private readonly string _filePath;
    private List<TradeOrder> _orders;

    public FileBackedOrderRepository(string filePath)
    {
        _filePath = filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
        _orders = Load();
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

    private void Persist()
    {
        lock (_gate)
        {
            var json = JsonSerializer.Serialize(_orders, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }

    public void Add(TradeOrder order)
    {
        lock (_gate)
        {
            _orders.Add(order);
            Persist();
        }
    }

    public IReadOnlyList<TradeOrder> GetAll()
    {
        lock (_gate)
        {
            return _orders.ToList();
        }
    }

    public TradeOrder? GetById(string id)
    {
        lock (_gate)
        {
            return _orders.FirstOrDefault(o => o.Id == id);
        }
    }
}