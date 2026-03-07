# Escale.Web

Admin and management web portal for the Escale Gas Station Management System. Built with ASP.NET Core MVC and Razor views.

## Tech Stack

- **ASP.NET Core 8** MVC with Razor Views
- **Bootstrap 5** for responsive UI
- **jQuery** + **DataTables** for interactive tables
- **Chart.js** for dashboard charts
- **SignalR** for real-time dashboard updates
- **Font Awesome 6** for icons

## Getting Started

### Prerequisites

- .NET 8 SDK
- Escale.API running at `https://localhost:7015`

### Setup

1. Ensure the API is running:
   ```bash
   cd ../Escale.API && dotnet run
   ```

2. Run the web app:
   ```bash
   cd Escale.Web && dotnet run
   ```

3. Open `https://localhost:7025` in your browser.

### Test Credentials

| Username    | Password       | Role       | Access                    |
|-------------|----------------|------------|---------------------------|
| superadmin  | superadmin123  | SuperAdmin | Organizations management  |
| admin       | admin123       | Admin      | Full organization access  |
| manager     | manager123     | Manager    | Station/department mgmt   |
| supervisor  | supervisor123  | Supervisor | Assigned stations only    |
| inventory   | inventory123   | Inventory  | Dashboard, inventory, reports |

> Cashier role is blocked from the web portal (mobile app only).

## Project Structure

```
Escale.Web/
├── Controllers/              # 13 MVC controllers
├── Views/                    # Razor views (25 total)
│   ├── Account/              # Login
│   ├── Dashboard/            # Dashboard with charts
│   ├── Organizations/        # SuperAdmin org management
│   ├── FuelTypes/            # Fuel type CRUD
│   ├── Stations/             # Station management
│   ├── Users/                # User management
│   ├── Customers/            # Customer + car + subscription mgmt
│   ├── Transactions/         # Transaction history + export
│   ├── Inventory/            # Stock levels + refills
│   ├── Reports/              # Report generation + export
│   ├── Settings/             # System settings + EBM config
│   ├── Profile/              # User profile + password change
│   ├── Home/                 # Home page
│   └── Shared/               # Layout, sidebar, navbar, partials
├── Models/
│   ├── Api/                  # 14 DTO files for API communication
│   └── *.cs                  # 11 ViewModels for views
├── Services/
│   ├── Interfaces/           # 11 service interfaces
│   └── Implementations/      # 12 service implementations
├── Filters/                  # RequireAuthAttribute
├── Handlers/                 # AuthenticatedHttpClientHandler (JWT injection)
├── Helpers/                  # TokenHelper (cookie/session management)
├── Configuration/            # ApiSettings
├── wwwroot/
│   ├── css/site.css          # 1500+ lines custom styling
│   ├── js/site.js            # SignalR integration, dashboard refresh
│   └── lib/                  # Bootstrap, jQuery, DataTables, Chart.js
└── Program.cs                # DI setup and middleware pipeline
```

## Controllers & Features

### AccountController — Authentication

- **Login** — Split-panel login page with branding
- **Logout** — Token revocation, cookie/session cleanup

### DashboardController — Organization Dashboard

- Real-time stats: total stations, daily revenue, credit sales
- Date range filtering (Today/Week/Month/Custom)
- Charts: sales trend (7 days), fuel type breakdown
- Top 5 stations ranking
- Recent transactions (auto-refreshing via SignalR)
- AJAX endpoint for live data refresh

### OrganizationsController — SuperAdmin Only

- List/create/edit/delete/restore organizations
- Manage stations per organization (add, toggle status)
- Configure EBM integration per organization
- Create/view organization admin users
- View/restore deleted fuel types

### StationsController — Station Management

- Station list with details (inventory, employees, stats)
- Station sales chart data (AJAX)
- Supervisor role filtering (assigned stations only)

### FuelTypesController — Fuel Type CRUD

- List, create, edit, delete fuel types
- EBM supply price tracking

### UsersController — User Management

- Paginated user list (20 per page) with search
- Create users with role and station assignment
- Edit, delete, toggle active status, reset password
- Roles: Admin, Manager, Supervisor, Inventory

### CustomersController — Customer Management

- Paginated list with type filter (Individual/Corporate)
- Customer profile: cars, subscriptions, statistics
- Car management: add, update, deactivate, reactivate
- Subscription: top-up, cancel, balance tracking
- Transaction history per customer with CSV export

### TransactionsController — Transaction History

- Paginated list (50 per page) with multi-filter
- Filters: station, fuel type, payment method, date range
- Transaction details with EBM receipt info
- Supervisor filtering (assigned stations only)
- CSV export with date range

### InventoryController — Stock Management

- Real-time stock levels by station
- Status indicators: Normal (green), Low (orange), Critical (red)
- Record refills with supplier info
- Reorder level management
- Refill history

### ReportsController — Report Generation

| Report     | Contents                                          |
|------------|---------------------------------------------------|
| Sales      | Daily breakdown, fuel type split, payment methods  |
| Inventory  | Stock levels, refill history, supplier costs       |
| Employee   | Transaction count, hours worked, total sales       |
| Customer   | Spending analysis, vehicle count, subscriptions    |
| Financial  | Revenue, VAT, refill costs, gross profit           |

All reports exportable as CSV.

### SettingsController — System Configuration

- Company info (name, tax rate, currency)
- Receipt customization (header, footer, auto-print)
- Stock thresholds (low stock %, critical %)
- EBM integration status and configuration
- Fuel price updates

### ProfileController — User Profile

- View/update profile (name, email, phone)
- Change password (forces re-login)

## Authentication & Authorization

### Token Flow

1. User submits credentials → POST to API `/api/auth/login`
2. API returns AccessToken + RefreshToken (JWTs)
3. Tokens stored in HttpOnly secure cookies (`escale_access_token`, `escale_refresh_token`)
4. `AuthenticatedHttpClientHandler` auto-injects token to every API request
5. On 401: attempts token refresh via `/api/auth/refresh-token`
6. If refresh fails: clears cookies, redirects to login

### Cookie Security

- `HttpOnly = true` (no JavaScript access)
- `Secure = true` (HTTPS only)
- `SameSite = Strict` (CSRF protection)
- Expiry: 7 days (Remember Me) or 1 hour (session)

### Role-Based Access

| Role        | Access                                                    |
|-------------|-----------------------------------------------------------|
| SuperAdmin  | Organizations only (redirected from Dashboard)            |
| Admin       | All features                                              |
| Manager     | All features                                              |
| Supervisor  | Dashboard, assigned stations, transactions (filtered)     |
| Inventory   | Dashboard, inventory, reports                             |
| Cashier     | Blocked (mobile only)                                     |

### Auth Filter

`RequireAuthAttribute` applied globally — checks for valid access token cookie, redirects to login if missing.

## API Communication

### Pattern

All API services extend `BaseApiService` with generic HTTP methods:
- `GetAsync<T>()`, `PostAsync<T>()`, `PutAsync<T>()`, `DeleteAsync()`

Responses parsed from `ApiResponse<T>` wrapper or ASP.NET `ProblemDetails` format.

### Configuration

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7015/api",
    "TimeoutSeconds": 30
  }
}
```

SSL validation bypassed in development (`ServerCertificateCustomValidationCallback`).

## Real-Time Updates (SignalR)

`site.js` connects to the API's SignalR hub for live notifications:

- **Dashboard auto-refresh** — Updates stats, charts, transactions on `sale_completed`, `inventory_changed`
- **Toast notifications** — Shows update alerts on data changes
- **Reconnection** — Exponential backoff on disconnect

### Dashboard AJAX Refresh

On notification or date range change:
1. Fetches `/Dashboard/Data` endpoint
2. Animates stat card value transitions
3. Updates recent transactions table
4. Updates top stations ranking
5. Refreshes Chart.js charts

## UI & Styling

### Layout Structure

- **Top Navbar** — Brand logo, notifications dropdown, user menu with role badge
- **Sidebar** — Role-based navigation menu (collapsible on mobile)
- **Main Content** — Page headers with breadcrumbs, content area

### Design System

| Element     | Style                                      |
|-------------|--------------------------------------------|
| Primary     | `#219ebc` (Ocean Blue)                     |
| Secondary   | `#f5d04b` (Golden Yellow)                  |
| Dark BG     | `#1a2332`                                  |
| Sidebar     | `#212b3d`                                  |
| Cards       | White with shadow, hover lift effect       |
| Tables      | DataTables with gradient headers           |
| Buttons     | Gradient backgrounds, hover transforms     |
| Forms       | Focus states, validation feedback          |
| Alerts      | Colored left border, dismissible           |

### Responsive

- Mobile-first approach
- Sidebar collapses on small screens
- Login layout stacks vertically on mobile
- Tables scroll horizontally on narrow viewports

### JavaScript Utilities (`site.js`)

| Function          | Purpose                              |
|-------------------|--------------------------------------|
| `showToast()`     | Toast notifications                  |
| `startLoading()`  | Button spinner state                 |
| `exportCsv()`     | CSV export with loading/error        |
| `escapeHtml()`    | XSS prevention                       |
| `animateValue()`  | Stat card fade transitions           |
| `formatNumber()`  | Locale-aware number formatting       |

## NuGet Packages

Only one explicit dependency:
- `Microsoft.Extensions.Http` 8.0.0 — HttpClient DI

All other features use ASP.NET Core 8 built-ins.

## Client-Side Libraries

| Library        | Version | Purpose                |
|----------------|---------|------------------------|
| Bootstrap      | 5.x     | Responsive layout, UI  |
| jQuery         | 3.x     | DOM manipulation       |
| DataTables     | 1.13    | Sortable/searchable tables |
| Chart.js       | Latest  | Dashboard charts       |
| SignalR        | 8.0     | Real-time updates      |
| Font Awesome   | 6.4     | Icons                  |

## Security

- **JWT Authentication** — Bearer tokens in HttpOnly secure cookies
- **CSRF Protection** — AntiForgery tokens on all POST forms
- **XSS Prevention** — Razor HTML encoding + JavaScript `escapeHtml()`
- **HTTPS Enforced** — `UseHttpsRedirection` middleware
- **Session Security** — 30-minute idle timeout, secure/httponly flags
- **Auto Token Refresh** — `AuthenticatedHttpClientHandler` refreshes on 401
- **Multi-Tenancy** — Data isolation enforced at API level

## Ports

| Protocol | Port  |
|----------|-------|
| HTTPS    | 7025  |
| HTTP     | 5078  |
