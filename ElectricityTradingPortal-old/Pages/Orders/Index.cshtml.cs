using ElectricityTradingPortal.Models;
using ElectricityTradingPortal.Repositories;
using ElectricityTradingPortal.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElectricityTradingPortal.Pages.Orders;

public class IndexModel : PageModel
{
    private readonly IOrderRepository _repository;

    public IndexModel(IOrderRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<TradeOrder> Orders { get; private set; } = Array.Empty<TradeOrder>();

    public IReadOnlyList<TradeOrder> Drafts { get; private set; } = Array.Empty<TradeOrder>();

    public void OnGet()
    {
        Orders = _repository.GetAll()
            .Where(o => o.Status == TradeOrderStatus.Submitted)
            .OrderByDescending(o => o.SubmittedAtUtc)
            .ToList();

        Drafts = DraftOrderStore.GetAll()
            .OrderByDescending(d => d.SubmittedAtUtc)
            .ToList();
    }
}