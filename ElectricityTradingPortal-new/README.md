# Electricity Trading Portal

A small ASP.NET Core (Razor Pages) application for submitting, listing, and
viewing electricity trade orders. Built as the starting point for a
hands-on workshop that evolves a working app toward a more operationally
responsible setup.

## What it does

- List submitted orders at `/Orders`
- Submit or save-as-draft a new order at `/Orders/Create`
- View order details at `/Orders/{id}`
- A JSON API at `GET /api/orders` and `POST /api/orders`

Orders are held in memory and reset when the app restarts. A background
service simulates periodic polling of an internal market-data feed.

## Run it

Requires the .NET 10 SDK.

```bash
dotnet run
```

Open <http://localhost:5000/Orders>. Because the app publishes on its
standard dev port, you will normally land on the orders list.

Submit a couple of orders from the **Submit Order** page — try one where
Volume &times; Price stays under the trading limit and one (in a Release
build) that exceeds it.

> Tip: the trading-limit check is skipped in `Debug` builds to keep local
> iteration fast, so run `dotnet run -c Release` if you want to observe the
> limit behaviour.

## Configuration

Settings live in `appsettings.json` and can be overridden with environment
variables at deploy time — see `DEPLOYMENT.md` for Linux and IIS examples.

---

## Presenter reference — planted improvement points

This section is for the presenter only and is intentionally **not** shown to
participants. The app was authored to look like reasonable first-version
code, with six discrete, subtle gaps that the workshop will close live. Each
is marked by a single neutral `TODO` comment. Locations are approximate and
may drift if you edit; search for the quoted text to confirm.

| # | Theme | Where | Marker |
|---|-------|-------|--------|
| 1 | Settings are repeated across environments and read ad hoc | `Program.cs:9` (loading `TradingLimits:MaxOrderValue` / `MarketDataFeed:Url`) | `// TODO: this value is the same everywhere...` |
| 2 | Data only survives in-memory | `Repositories/InMemoryOrderRepository.cs:10` | `// TODO: orders don't survive a restart...` |
| 3 | Little/unstructured visibility | `Services/MarketDataFeedClient.cs:34` (also `Console.WriteLine` in `Services/OrderValidationService.cs:23` and `Pages/Orders/Create.cshtml.cs:71`) | `// TODO: if this call fails, there's not much here...` |
| 4 | Dev/Release behaviour diverges | `Services/OrderValidationService.cs:16` (`#if DEBUG` block) | `// TODO: might be worth checking this behaves the same way...` |
| 5 | Drafts are process-local only | `Services/DraftOrderStore.cs:9` (static dictionary) | `// TODO: drafts are only visible from the instance that created them...` |
| 6 | Graceful shutdown is slow | `Services/MarketDataPollingService.cs:25` (delay ignores the shutdown signal) | `// TODO: shutdown seems to take a while...` |

Suggested outline if you want to timebox the session:

1. **10 min** — intro + `dotnet run`, walk the UI once.
2. **Seed 4/1** — make the trading limit honour the build configuration and
   surface the value from options.
3. **Seed 6** — observe a slow exit on `Ctrl+C`, then respect the shutdown
   token in the background service.
4. **Seed 3** — add structured logging so failures are diagnosable.
5. **Seed 2** — swap the repository for one that persists.
6. **Seed 5** — decide whether process-local drafts are acceptable and
   document the choice.

Each seed is independent; doing one does not break the others.