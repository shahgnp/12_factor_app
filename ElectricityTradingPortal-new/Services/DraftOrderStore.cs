using System.Text.Json;
using ElectricityTradingPortal.Models;

namespace ElectricityTradingPortal.Services;

public static class DraftOrderStore
{
    private static readonly object Gate = new();
    private static readonly string FilePath = Path.Combine(
        Directory.GetCurrentDirectory(), "data", "drafts.json");

    public static void Save(TradeOrder draft)
    {
        lock (Gate)
        {
            var drafts = Load();
            drafts[draft.Id] = draft;
            Persist(drafts);
        }
    }

    public static TradeOrder? Get(string id)
    {
        lock (Gate)
        {
            return Load().TryGetValue(id, out var draft) ? draft : null;
        }
    }

    public static IReadOnlyList<TradeOrder> GetAll()
    {
        lock (Gate)
        {
            return Load().Values.ToList();
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

    private static void Persist(Dictionary<string, TradeOrder> drafts)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(drafts, new JsonSerializerOptions { WriteIndented = true }));
    }
}