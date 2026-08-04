using ElectricityTradingPortal.Models;
using ElectricityTradingPortal.Repositories;
using ElectricityTradingPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElectricityTradingPortal.Pages.Orders;

public class DetailsModel : PageModel
{
    private readonly IOrderRepository _repository;

    public DetailsModel(IOrderRepository repository)
    {
        _repository = repository;
    }

    public TradeOrder? Order { get; private set; }

    public IActionResult OnGet(string id)
    {
        var order = _repository.GetById(id)
            ?? DraftOrderStore.Get(id);

        if (order is null)
        {
            return NotFound();
        }

        Order = order;
        return Page();
    }
}