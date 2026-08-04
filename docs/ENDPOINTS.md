# Endpoints & Types Reference

All HTTP endpoints and public types exposed by the Electricity Trading Portal.

## Base URL

Local development: `http://localhost:5000` (or `https://localhost:5001` when
launch profile is used).

---

## Server-rendered pages (Razor Pages)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | Home page with links into the portal |
| GET | `/Orders` | List all **Submitted** orders, newest first |
| GET | `/Orders/Create` | New-order form. Optional `?draft={id}` pre-loads a saved draft |
| POST | `/Orders/Create?handler=Submit` | Submits an order (form post). Validates the trading limit, then redirects to the order's details page |
| POST | `/Orders/Create?handler=SaveDraft` | Saves the current form values as an in-progress draft |
| GET | `/Orders/{id}` | Details for a single order or draft |

Form posts require the antiforgery token embedded in the rendered form.