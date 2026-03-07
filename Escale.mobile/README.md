# Escale.mobile

Cashier mobile application for the Escale Gas Station Management System. Built with .NET MAUI for cross-platform deployment.

## Tech Stack

- **.NET 9** MAUI (Android, iOS, Windows, macOS)
- **CommunityToolkit.Mvvm** (MVVM pattern with source generators)
- **SignalR Client** for real-time notifications
- **System.Text.Json** for serialization
- **Material Design Icons** for UI

## Getting Started

### Prerequisites

- .NET 9 SDK with MAUI workload
- Android emulator or physical device (for Android)
- Escale.API running at `https://localhost:7015`

### Setup

1. Ensure the API is running:
   ```bash
   cd ../Escale.API && dotnet run
   ```

2. Run the mobile app:
   ```bash
   dotnet build -t:Run -f net9.0-android
   ```

### API Connection

| Platform | Base URL                      |
|----------|-------------------------------|
| Android  | `https://10.0.2.2:7015/api`   |
| iOS      | `https://localhost:7015/api`   |
| Windows  | `https://localhost:7015/api`   |

Android emulator uses `10.0.2.2` to reach the host machine's `localhost`. Self-signed SSL certificates are accepted in DEBUG builds.

### Test Credentials

| Username | Password     | Role    |
|----------|--------------|---------|
| cashier  | cashier123   | Cashier |
| admin    | admin123     | Admin   |
| manager  | manager123   | Manager |

## Project Structure

```
Escale.mobile/
├── App.xaml / App.xaml.cs           # App lifecycle, shell pre-creation, connection pre-warm
├── AppShell.xaml / AppShell.xaml.cs  # Tab navigation (5 tabs) + route registration
├── MauiProgram.cs                   # DI container setup
├── Models/                          # Data models (Sale, Customer, Stock, etc.)
├── ViewModels/                      # MVVM ViewModels (12 total)
├── Views/                           # XAML pages (12 total)
├── Services/                        # API client, SignalR, AppState
├── Converters/                      # XAML value converters (6 total)
├── Helpers/                         # IconFont constants
├── Platforms/                       # Platform-specific code
│   ├── Android/                     # MainActivity
│   ├── iOS/                         # AppDelegate
│   ├── MacCatalyst/
│   └── Windows/
└── Resources/                       # Images, fonts, styles
```

## Navigation

### Tab Bar (AppShell)

| Tab        | Page             | Description              |
|------------|------------------|--------------------------|
| Dashboard  | DashboardPage    | Sales summary, alerts    |
| New Sale   | NewSalePage      | Create transactions      |
| Stock      | StockPage        | Inventory levels         |
| History    | TransactionsPage | Transaction history      |
| Profile    | ProfilePage      | User settings, clock     |

### Sale Flow

```
NewSale → SubscriptionCustomerSearch (Credit payment)
       → CustomerInfo (other payments)
       → SalePreview → Success Popup
```

### Other Routes

- `StationSelection` — Shown after login for Admin/Manager to pick station
- `CustomerSearch` — Search registered credit customers
- `ChangePassword` — Change password from Profile

## Pages & ViewModels

### LoginPage / LoginViewModel

Login with username/password. Supports "Remember Me" (username in Preferences, password in SecureStorage). Routes Cashier directly to Dashboard, Admin/Manager to StationSelection.

### StationSelectionPage / StationSelectionViewModel

Pick a station to work at. Option to remember choice. Shown for Admin/Manager roles only.

### DashboardPage / DashboardViewModel

- Today's sales total and transaction count
- Low stock alerts (yellow banner)
- Quick actions: New Sale, Check Stock
- Recent transactions with EBM receipt links
- Pull-to-refresh with skeleton loading
- Auto-refresh via SignalR on: sale_completed, inventory_changed, fuel_types_changed, station_changed

### NewSalePage / NewSaleViewModel

- Fuel type picker (shows price per liter)
- Amount (RWF) and Liters inputs with auto-calculation
- Payment method: Cash, MobileMoney, Card, Credit
- Credit payment routes to SubscriptionCustomerSearch
- Other payments route to CustomerInfo

### CustomerInfoPage / CustomerInfoViewModel

- Optional customer name, phone, TIN fields (side by side layout)
- "Skip" creates a walk-in customer
- "Continue" saves customer info to sale

### SubscriptionCustomerSearchPage / SubscriptionCustomerSearchViewModel

- Plate number + PIN verification
- Shows customer details, vehicle info, subscription balance
- Validates sufficient balance for sale amount
- Routes to SalePreview with subscription data

### CustomerSearchPage / CustomerSearchViewModel

- Search registered customers by name, phone, or plate
- Tap to select customer for credit sale

### SalePreviewPage / SalePreviewViewModel

- Full sale summary: fuel, liters, price, subtotal, VAT (18%), total
- Customer and payment info
- Subscription balance (if applicable)
- Edit and Confirm buttons
- Success popup overlay with:
  - Receipt number
  - Print Receipt button (opens EBM receipt in browser)
  - New Sale button

### StockPage / StockViewModel

- Stock levels per fuel type with progress bars
- Color-coded: red (< 20%), orange (20-50%), green (> 50%)
- Pull-to-refresh
- Auto-refresh via SignalR on sale_completed, inventory_changed

### TransactionsPage / TransactionsViewModel

- Date range filter (From/To pickers)
- Transaction list with fuel type, amount, payment method, EBM status
- Tap to view details with EBM receipt link
- Summary card: count and total

### ProfilePage / ProfileViewModel

- User info (name, role, station)
- Today's stats (transactions, amount)
- Clock In/Out with shift tracking
- Switch Station (Admin/Manager only)
- Shift Summary, Change Password, Logout

### ChangePasswordPage / ChangePasswordViewModel

- Current, new, confirm password fields with toggle visibility
- Validation: min 6 chars, must match, must differ from current
- Logs out on success

## Services

### ApiService (Singleton)

HTTP client for all API communication.

| Endpoint                   | Method | Purpose                |
|----------------------------|--------|------------------------|
| `/auth/login`              | POST   | Authenticate           |
| `/auth/change-password`    | POST   | Change password        |
| `/dashboard/summary`       | GET    | Dashboard data         |
| `/fueltypes`               | GET    | Fuel types list        |
| `/sales`                   | POST   | Submit sale            |
| `/transactions`            | GET    | Transaction history    |
| `/inventory`               | GET    | Stock levels           |
| `/customers/search`        | GET    | Customer search        |
| `/shifts/summary`          | GET    | Shift statistics       |
| `/shifts/clock`            | POST   | Clock in/out           |
| `/subscriptions/lookup`    | POST   | Car subscription check |
| `/health`                  | GET    | Connection pre-warm    |

**Features:**
- SSL bypass for dev (self-signed certs)
- 180s default timeout, 120s for sale submission (EBM processing)
- Fuel type caching (5-minute TTL, invalidated via SignalR)
- Connection pre-warming on app startup

### AppState (Singleton)

Global state manager accessible via `AppState.Instance`.

| Property         | Type          | Description              |
|------------------|---------------|--------------------------|
| `CurrentUser`    | UserInfo?     | Authenticated user       |
| `SelectedStation`| StationInfo?  | Active station           |
| `CurrentSale`    | SaleModel?    | In-progress sale         |
| `AuthToken`      | string?       | JWT bearer token         |
| `ShiftStartTime` | DateTime?     | Clock-in timestamp       |
| `IsLoggedIn`     | bool          | Computed: user + token   |
| `IsClockedIn`    | bool          | Computed: shift active   |

### SignalRService (Singleton)

Real-time notifications via SignalR hub at `/hubs/escale`.

- JWT authentication via access token
- Auto-reconnect with backoff (0s, 2s, 5s, 10s, 30s)
- Fires `OnDataChanged` event per notification type

**Notification Types:**
| Constant              | Triggers                        |
|-----------------------|---------------------------------|
| `fuel_types_changed`  | Invalidates fuel cache          |
| `sale_completed`      | Refreshes dashboard/stock       |
| `inventory_changed`   | Refreshes stock page            |
| `user_changed`        | Refreshes profile               |
| `station_changed`     | Refreshes dashboard             |
| `settings_changed`    | Refreshes settings              |

## Models

### SaleModel

Core sale object passed through the sale flow:
- `FuelTypeId`, `FuelType`, `PricePerLiter`
- `AmountRWF`, `Liters` (auto-calculated)
- `PaymentMethod` (Cash/MobileMoney/Card/Credit)
- `Customer` (CustomerInfo — optional)
- `SubscriptionId` (for subscription payments)
- Computed: `Subtotal`, `VAT` (18%), `Total`

### CustomerInfo

- `Name`, `PhoneNumber`, `TIN`, `PlateNumber`, `VehicleModel`
- `IsWalkIn` — true if no registered customer ID
- `ActiveSubscriptionId`, `SubscriptionBalance`, `SubscriptionExpiryDate`

### FuelTypeOption

- `Id`, `Name`, `PricePerLiter`
- `Icon`, `BadgeColor`, `DisplayText` — UI properties

### StockLevel

- `FuelType`, `CurrentLevel`, `Capacity`
- `PercentageRemaining`, `Status`, `StatusColor` — computed

## Value Converters

| Converter                         | Purpose                         |
|-----------------------------------|---------------------------------|
| `StringToBoolConverter`           | Non-empty string to true        |
| `InvertedBoolConverter`           | Negate boolean                  |
| `PercentToDecimalConverter`       | 0-100 to 0.0-1.0 (ProgressBar) |
| `ObjectToBoolConverter`           | Non-null to true                |
| `BoolToClockButtonColorConverter` | Green (in) / Red (out)          |
| `BoolToClockInOutTextConverter`   | "Clock In" / "Clock Out"        |

## DI Registration (MauiProgram.cs)

- **Singletons:** ApiService, AppState, SignalRService
- **Transients:** All 12 ViewModels and 12 Pages

## Performance Optimizations

1. **SSL Pre-Warming** — `/health` request on startup completes SSL handshake before login
2. **AppShell Pre-Creation** — Shell built in background while user is on login page
3. **Fuel Type Caching** — 5-minute TTL, invalidated via SignalR
4. **Fire-and-Forget** — SignalR, fuel types, and credentials saved in parallel after login
5. **Skeleton Loading** — Placeholder UI while data loads
6. **Single Dashboard Load** — Guard prevents duplicate API calls

## Styling

| Color     | Hex       | Usage           |
|-----------|-----------|-----------------|
| Primary   | `#1E3A8A` | Headers, links  |
| Success   | `#10B981` | Sales, positive |
| Warning   | `#F59E0B` | Alerts          |
| Danger    | `#EF4444` | Errors, logout  |
| Background| `#F8F9FA` | Page background |
| Cards     | `#FFFFFF` | Card surfaces   |

**Fonts:** OpenSans (Regular, Semibold), Material Design Icons

## Security

- JWT stored in memory only (AppState), cleared on logout
- Password saved in platform SecureStorage (only with "Remember Me")
- SSL certificate validation enforced in release builds
- Bearer token sent on all authenticated requests and SignalR connections
