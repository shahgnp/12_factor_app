using ElectricityTradingPortal.Models;
using ElectricityTradingPortal.Repositories;
using ElectricityTradingPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElectricityTradingPortal.Pages.Orders;

public class CreateModel : PageModel
{
    private readonly IOrderRepository _repository;
    private readonly OrderValidationService _validation;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        IOrderRepository repository,
        OrderValidationService validation,
        ILogger<CreateModel> logger)
    {
        _repository = repository;
        _validation = validation;
        _logger = logger;
    }

    [BindProperty]
    public TradeOrder Form { get; set; } = new();

    [BindProperty]
    public string? DraftId { get; set; }

    public string? DraftSavedId { get; private set; }

    public void OnGet(string? draft = null)
    {
        if (draft is null)
        {
            return;
        }

        var existing = DraftOrderStore.Get(draft);
        if (existing is not null)
        {
            DraftId = draft;
            Form = existing;
        }
    }

    public IActionResult OnPostSubmit()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var order = new TradeOrder
        {
            Trader = Form.Trader,
            Commodity = Form.Commodity,
            Volume = Form.Volume,
            Price = Form.Price,
            Side = Form.Side,
            SubmittedAtUtc = DateTime.UtcNow,
            Status = TradeOrderStatus.Submitted
        };

        if (!_validation.IsWithinTradingLimit(order))
        {
            order.Status = TradeOrderStatus.Rejected;
            _repository.Add(order);
            ModelState.AddModelError(string.Empty,
                $"Order value {order.Value:0.00} exceeds the trading limit. Order recorded as rejected.");
            return Page();
        }

        _repository.Add(order);
        _logger.LogInformation("Order {OrderId} submitted by {Trader}", order.Id, order.Trader);
        return RedirectToPage("/Orders/Details", new { id = order.Id });
    }

    public IActionResult OnPostSaveDraft()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var draftId = string.IsNullOrEmpty(DraftId) ? Guid.NewGuid().ToString("N") : DraftId;

        var draft = new TradeOrder
        {
            Id = draftId,
            Trader = Form.Trader,
            Commodity = Form.Commodity,
            Volume = Form.Volume,
            Price = Form.Price,
            Side = Form.Side,
            Status = TradeOrderStatus.Draft
        };

        DraftOrderStore.Save(draft);
        DraftId = draftId;
        DraftSavedId = draftId;
        return Page();
    }
}