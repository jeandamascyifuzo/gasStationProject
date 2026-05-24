# Session Changes — 2026-05-23

## 1. EBM Authentication — 401 Unauthorized Fix

**Problem:** Every request to YegoBox API (`apihub.yegobox.com`) returned `401 Unauthorized` because the `EBM` HttpClient was registered with no authentication header.

**Root cause:** `OrganizationSettings` had no API key field and `ServiceCollectionExtensions.cs` registered the EBM HttpClient with only a timeout.

**Fix:** YegoBox uses HTTP Basic Authentication (`admin:admin`). Added credentials to `appsettings.json` and configured the EBM HttpClient to attach `Authorization: Basic YWRtaW46YWRtaW4=` on every request.

**Files changed:**
- `Escale.API/appsettings.json` — added `EBMSettings` section
- `Escale.API/Extensions/ServiceCollectionExtensions.cs` — updated EBM HttpClient to read credentials and set Basic auth header

```json
// appsettings.json
"EBMSettings": {
  "Username": "admin",
  "Password": "admin"
}
```

---

## 2. EBM Error Message — RRA Connection Refused

**Problem:** When YegoBox returned `500 — "Failed to call RRA API: Connection refused"`, the cashier saw the misleading message *"Cannot connect to EBM server. Please check your internet connection"* — blaming the cashier's internet when it's actually YegoBox's server that can't reach the RRA API.

**Root cause:** `ParseEbmError()` in `SaleService.cs` checked `lower.Contains("connection")` before checking for `"500"`, so YegoBox's "Connection refused" matched the wrong branch.

**Fix:** Added a specific RRA check before the generic connection check.

**Files changed:**
- `Escale.API/Services/Implementations/SaleService.cs` — added RRA-specific branch in `ParseEbmError()`

**New message shown to cashier:**
> *"EBM receipt could not be submitted — the RRA tax server is temporarily unreachable. Please try again in a few moments."*

---

## 3. VAT Calculation — Tax-Inclusive Fix

**Problem:** The API calculated VAT using the tax-exclusive formula (`total × 0.18`) but the 3000 RWF/L pump price already includes VAT. The mobile app used the correct tax-inclusive formula. This caused the stored subtotal and VAT in the DB to not match what the cashier saw on the confirmation screen.

| | Mobile (correct) | API (wrong) |
|---|---|---|
| Formula | `total × (18/118)` | `total × 0.18` |
| VAT on 2010 RWF | 307 RWF ✓ | 362 RWF ✗ |
| Subtotal | 1703 RWF ✓ | 1648 RWF ✗ |

**Fix:** Updated the API to use the same inclusive formula as the mobile, rounded to 0 decimal places (RWF has no cents).

**Files changed:**
- `Escale.API/Services/Implementations/SaleService.cs` — changed VAT formula

```csharp
// Before (wrong — tax exclusive)
var vatAmount = Math.Round(total * BusinessRules.VATRate, 2);

// After (correct — tax inclusive, matches mobile)
var vatAmount = Math.Round(total * (18m / 118m), 0);
```

---

## 4. Sale Total — Rounding Overcharge Fix

**Problem:** When a cashier entered 2000 RWF, the mobile calculated `2000 ÷ 3000 = 0.6667L`, displayed as `0.67L`. The API then recalculated `0.67 × 3000 = 2010 RWF` — charging the customer 10 RWF more than they entered.

**Root cause:** The mobile rounded liters to 2 decimal places (`F2`) and only sent `Liters` to the API. The API had no knowledge of the original amount and recalculated from rounded liters.

**Fix:** Added `AmountRWF` (optional) to the sale request. When provided, the API uses it directly as the total instead of recalculating from liters. Mobile now sends the original entered amount alongside the liters.

**Files changed:**
- `Escale.API/DTOs/Sales/SaleDtos.cs` — added `AmountRWF?` to `CreateSaleRequestDto`
- `Escale.API/Services/Implementations/SaleService.cs` — use `AmountRWF` as total when present
- `Escale.mobile/Services/ApiService.cs` — added `AmountRWF` to `CreateSaleRequest` and send it in `SubmitSaleAsync`

**Result with 2000 RWF entered:**

| | Before | After |
|---|---|---|
| Total | 2010 RWF ✗ | 2000 RWF ✓ |
| VAT | 307 RWF | 305 RWF |
| Subtotal | 1703 RWF | 1695 RWF |
| Liters | 0.67 L | 0.67 L |

---

## 5. Debug — EBM Auth Header Logging

**Added temporarily** to `EBMService.SendSaleReceiptAsync()` to verify the Authorization header is being sent correctly.

```
[INF] EBM Auth header: Basic YWRtaW46YWRtaW4=
```

**File:** `Escale.API/Services/Implementations/EBMService.cs`
> Remove these two log lines once confirmed working.

---

## Outstanding Issue — RRA API Unreachable (YegoBox Side)

Sales still cannot be completed because YegoBox's server returns:
```
500 Internal Server Error — "Failed to call RRA API: Connection refused"
```

This was confirmed via direct Postman test with correct auth and full payload — the request reaches YegoBox fine but YegoBox cannot forward it to the RRA EBM server. **This is a YegoBox infrastructure issue, not a code issue.**

**Status:** Reported to YegoBox support. Waiting for fix.

**Potential next step:** Implement Option B — save sale with `EBMPending = true` and retry EBM submission later, so cashiers are not blocked during RRA outages.
