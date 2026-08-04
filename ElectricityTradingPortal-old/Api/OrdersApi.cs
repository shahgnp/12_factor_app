using System.ComponentModel.DataAnnotations;
using ElectricityTradingPortal.Models;
using ElectricityTradingPortal.Repositories;
using ElectricityTradingPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElectricityTradingPortal.Api;

public static class OrdersApi
{
    public static IEndpointRouteBuilder MapOrdersApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/orders");

        group.MapGet("/", (IOrderRepository repository) =>
            TypedResults.Ok(repository.GetAll().Where(o => o.Status == TradeOrderStatus.Submitted)));

        group.MapPost("/", async (
            [FromBody] CreateOrderRequest request,
            IOrderRepository repository,
            OrderValidationService validation,
            CancellationToken cancellationToken) =>
        {
            var order = new TradeOrder
            {
                Trader = request.Trader,
                Commodity = request.Commodity,
                Volume = request.Volume,
                Price = request.Price,
                Side = request.Side,
                SubmittedAtUtc = DateTime.UtcNow,
                Status = TradeOrderStatus.Submitted
            };

            if (!validation.IsWithinTradingLimit(order))
            {
                order.Status = TradeOrderStatus.Rejected;
                repository.Add(order);
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["order"] = new[] { "Order value exceeds the configured trading limit." }
                });
            }

            repository.Add(order);
            return Results.Created($"/orders/{order.Id}", order);
        });

        return routes;
    }

    public sealed class CreateOrderRequest
    {
        [Required]
        public string? Trader { get; set; }

        [Required]
        public string Commodity { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Volume { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public OrderSide Side { get; set; }
    }
}