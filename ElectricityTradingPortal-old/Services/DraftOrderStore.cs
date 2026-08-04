using ElectricityTradingPortal.Models;

namespace ElectricityTradingPortal.Services;

public static class DraftOrderStore
{
    private static readonly Dictionary<string, TradeOrder> Drafts = new();

    // TODO: drafts are only visible from the instance that created them — hasn't been an issue yet

    public static void Save(TradeOrder draft) => Drafts[draft.Id] = draft;

    public static TradeOrder? Get(string id) => Drafts.TryGetValue(id, out var draft) ? draft : null;

    public static IReadOnlyList<TradeOrder> GetAll() => Drafts.Values.ToList();
}