using System.Text.Json;
using ElectricityTradingPortal.Models;

namespace ElectricityTradingPortal.Services;

public static class DraftOrderStore
{
    private static readonly object Gate = new();
    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "drafts.json");
    private static Dictionary<string, TradeOrder> Drafts = Load();

    public static void Save(TradeOrder draft)
    {
        lock (Gate)
        {
            Drafts[draft.Id] = draft;
            Persist();
        }
    }

    public static TradeOrder? Get(string id)
    {
        lock (Gate)
        {
            return Drafts.TryGetValue(id, out var draft) ? draft : null;
        }
    }

    public static IReadOnlyList<TradeOrder> GetAll()
    {
        lock (Gate)
        {
            return Drafts.Values.ToList();
        }
    }

    private static Dictionary<string, TradeOrder> Load()
    {
        if (!File.Exists(FilePath))
        {
            return new Dictionary<string, TradeOrder>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, TradeOrder>>(File.ReadAllText(FilePath))
                ?? new Dictionary<string, TradeOrder>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, TradeOrder>();
        }
    }

    private static void Persist()
    {
        lock (Gate)
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(Drafts, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}