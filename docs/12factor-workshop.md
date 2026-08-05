---
marp: true
theme: default
paginate: true
footer: Electricity Trading Portal · Cloud-Ready Workshop
---

<!-- _class: lead -->

## The starting point

The **Electricity Trading Portal** is a real, working ASP.NET Core (Razor Pages)
app:

- Submit, list, and view electricity trade orders
- JSON API (`/api/orders`) alongside server-rendered pages
- In-memory storage, a background poller, and a market-data feed

It builds and runs. But there is a gap between "runs on my machine" and
"runs reliably in the cloud."

---

## The Twelve Factors

| # | Factor | Idea |
|---|--------|------|
| 1 | Codebase | One codebase, many deploys |
| 2 | Dependencies | Declare and isolate dependencies explicitly |
| 3 | Config | Keep configuration in the environment |
| 4 | Backing services | Treat supporting services as attached resources |
| 5 | Build, release, run | Separate the build, release, and run stages |
| 6 | Processes | Run apps as stateless processes |

---

## The Twelve Factors

| # | Factor | Idea |
|---|--------|------|
| 7 | Port binding | Export services by binding to a port |
| 8 | Concurrency | Scale out via the process model |
| 9 | Disposability | Fast startup and graceful shutdown |
| 10 | Dev/prod parity | Keep environments as similar as possible |
| 11 | Logs | Treat logs as event streams |
| 12 | Admin processes | Run admin tasks as one-off processes |

---

<!-- _class: lead -->

# Deep Dive

The six factors this scaffold was missing.

---

<!-- _class: lead -->

## Factor 3 · Config

### Hard-coded values, read ad hoc

---

## Factor 3 — Config · Before

**What was lacking**

- Trading limits and the market-feed URL hard-coded in `appsettings.json`
- Read via bare `IConfiguration["TradingLimits:MaxOrderValue"]` string lookups
  in `Program.cs`, with inline fallback defaults
- No type safety, no per-environment override — the same value everywhere

```csharp
var tradingLimitRaw = configuration["TradingLimits:MaxOrderValue"];
var tradingLimit = decimal.TryParse(tradingLimitRaw, out var parsed) ? parsed : 500000m;
```

```json
"TradingLimits": { "MaxOrderValue": 500000 },
"MarketDataFeed": { "Url": "https://internal-market-feed.example.local/api" }
```

`// TODO: this value is the same everywhere right now ...`

---

## Factor 3 — Config · After

Env-name-your-values, don't code them:

```csharp
builder.Services.Configure<TradingOptions>(configuration.GetSection("TradingLimits"));
builder.Services.Configure<MarketDataFeedOptions>(configuration.GetSection("MarketDataFeed"));
```

- Typed options classes (`TradingOptions`, `MarketDataFeedOptions`)
- Values read through `IOptions<T>` — no magic strings
- Override per environment with `appsettings.<env>.json` or environment
  variables (`TradingLimits__MaxOrderValue`, ...)

---

## Factor 3 — Config · Benefits

- **Credentials and URLs stay out of source control** — safe to share the code
- **One build runs anywhere** — promote the same artifact across environments
- **No silent drift** — every difference is an explicit override
- **Compiler-checked** — misspelling a key is caught at build time, not runtime

---

<!-- _class: lead -->

## Factor 6 · Processes

### Stateless processes that forget everything on exit

---

## Factor 6 — Processes · Before

**What was lacking**

- `InMemoryOrderRepository` keeps every order in a `ConcurrentDictionary`
- Registered as a singleton — state lives inside the process
- One restart later, all orders are gone

```csharp
// TODO: orders don't survive a restart — fine for now, but worth flagging ...
```

**Consequence:** you can't run two replicas (each has different data) and you
can't restart without losing data.

---

## Factor 6 — Processes · After

Make the process stateless; move state to an attached, durable resource:

- New `FileBackedOrderRepository` implementing the same `IOrderRepository`
- Orders persist to `./data/orders.json` (could be a database instead)
- Swapped in via one line of DI — callers unchanged

```csharp
builder.Services.AddSingleton<IOrderRepository>(
    new FileBackedOrderRepository(storagePath));
```

---

## Factor 6 — Processes · Benefits

- **Orders survive restarts** — no more surprise data loss
- **Horizontal scaling works** — every instance reads/writes the same store
- **No sticky sessions** — any instance can serve any request
- **Swap-by-config** — the interface means you can move to a real database
  later without touching callers

---

<!-- _class: lead -->

## Factor 8 · Concurrency

### Per-instance state that isn't shared

---

## Factor 8 — Concurrency · Before

**What was lacking**

- Drafts live in a `static Dictionary<string, TradeOrder>` inside the process
- A draft is only visible to the instance that created it

```csharp
// TODO: drafts are only visible from the instance that created them ...
```

**Consequence:** with more than one instance (or a restart), the "save draft" and
"continue editing" feature silently breaks.

---

## Factor 8 — Concurrency · After

Decouple state from the single process:

- Drafts persist (e.g. `drafts.json`) so they survive a restart
- Longer-term: consolidate drafts behind the same repository/store used for
  submitted orders, so all instances share them
- Or deliberately scope the feature to one session and say so in the UI

---

## Factor 8 — Concurrency · Benefits

- **Consistent behaviour under scale-out** — no more "works on one node"
- **Survives restarts** — in-progress work isn't lost
- **Deliberate product decisions** — you choose, instead of inheriting a bug
- **Clearer guarantees** for whoever writes the load-balancer config

---

<!-- _class: lead -->

## Factor 9 · Disposability

### Fast start, graceful shutdown

---

## Factor 9 — Disposability · Before

**What was lacking**

- The background poller runs `while (true)` and ignores the shutdown signal
- `Task.Delay(..., CancellationToken.None)` keeps it alive
- Shutting down the app took ~30 seconds (the shutdown timeout)

```csharp
// TODO: shutdown seems to take a while — haven't dug into why yet
```

**Consequence:** deploys, scale-downs, and rollbacks are slow and feel risky.

---

## Factor 9 — Disposability · After

Observe the cancellation token everywhere:

```csharp
while (!stoppingToken.IsCancellationRequested)
{
    var snapshot = await _feed.GetBestPeakPriceAsync(stoppingToken);
    await Task.Delay(interval, stoppingToken);   // token plumbed through
}
```

The loop now exits cleanly on `Ctrl+C`; shutdown dropped from ~30s to **~1s**.

---

## Factor 9 — Disposability · Benefits

- **Fast, safe deploys** — instances come and go quickly
- **Instant rollbacks** — old versions stop in a second
- **Graceful draining** — in-flight requests complete, nothing is dropped
- **Cheaper to run** — cloud orchestrators can bin-pack and recycle at will

---

<!-- _class: lead -->

## Factor 10 · Dev/prod Parity

### Same behaviour in every build

---

## Factor 10 — Dev/prod Parity · Before

**What was lacking**

- The trading-limit check was skipped in `Debug` builds behind `#if DEBUG`

```csharp
#if DEBUG
    // skip the limit check so local journeys stay quick
    return true;
#endif
```

`// TODO: might be worth checking this behaves the same way in every build config ...`

**Consequence:** an over-limit bug could slip through locally and only surface in
production.

---

## Factor 10 — Dev/prod Parity · After

Remove the build-conditional behaviour — one code path for every build:

```csharp
public bool IsWithinTradingLimit(TradeOrder order)
{
    if (order.Value > _maxOrderValue) { /* reject + log */ return false; }
    return true;
}
```

If a "fast local mode" is genuinely wanted, gate it on an explicit config flag
(defaulting the same everywhere) instead of a compiler symbol.

---

## Factor 10 — Dev/prod Parity · Benefits

- **What you test locally is what ships** — no Debug/Release surprise
- **The same validation protects every environment**
- **No last-second "it works here though"** during release
- **Simpler reasoning** — one path, not several

---

<!-- _class: lead -->

## Factor 11 · Logs

### Logs as a searchable event stream

---

## Factor 11 — Logs · Before

**What was lacking**

- A couple of scattered `Console.WriteLine` calls
- No severity, no structure, no correlation, no exception detail
- The market-feed client swallowed failures and returned `null` — nothing to
  investigate

```csharp
// TODO: if this call fails, there's not much here to help figure out why later
```

**Consequence:** when something breaks in the field, you have almost nothing to
diagnose with.

---

## Factor 11 — Logs · After

Use structured logging through `ILogger<T>` and never hide failures:

```csharp
_logger.LogError(ex,
    "Market data feed call failed for {Endpoint}; returning fallback", endpoint);
```

- Named fields (`{Endpoint}`, `{OrderId}`) are searchable
- Severity levels, exceptions, and (via the framework) request correlation id
- Failures are logged, not silently swallowed

---

## Factor 11 — Logs · Benefits

- **Fix problems faster** — search by order id, trader, endpoint
- **Operational visibility** — severity and volume tell you what's wrong
- **Centralisable** — the stream can feed ELK / cloud logging unchanged
- **Accountability** — you know *which* call failed and *why*

---

<!-- _class: lead -->

## Recap

The six factors this app was missing, in one map

---

## Summary — before → after

| Factor | Before (the scaffold) | After (the improvement) |
|--------|------------------------|--------------------------|
| 3 Config | hard-coded, string lookups | typed `IOptions<T>`, env overrides |
| 6 Processes | in-memory repo, data lost | durable `IOrderRepository` |
| 8 Concurrency | process-local drafts | persisted / shared drafts |
| 9 Disposability | ~30 s shutdown | ~1 s graceful shutdown |
| 10 Dev/prod parity | `#if DEBUG` skips checks | one code path everywhere |
| 11 Logs | scattered `Console.WriteLine` | structured `ILogger` events |

---