# Escale.API

REST API backend for the Escale Gas Station Management System. Built with .NET 8, Entity Framework Core, and SQL Server.

## Tech Stack

- **.NET 8** Web API
- **Entity Framework Core 8** with SQL Server
- **JWT** authentication (access + refresh tokens)
- **SignalR** for real-time notifications
- **AutoMapper** for entity-to-DTO mapping
- **FluentValidation** for request validation
- **BCrypt** for password hashing
- **Serilog** for structured logging (console + rotating file)
- **Swagger/OpenAPI** for API documentation

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full instance)

### Setup

1. Update the connection string in `appsettings.json` if needed:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=EscaleDb;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
   }
   ```

2. Run the API:
   ```bash
   dotnet run
   ```
   The database is created and seeded automatically on first startup.

3. Open Swagger UI at `https://localhost:7015`

### Test Credentials (Seeded)

| Username    | Password       | Role       | Organization     |
|-------------|----------------|------------|------------------|
| superadmin  | superadmin123  | SuperAdmin | System           |
| admin       | admin123       | Admin      | Escale Petroleum |
| manager     | manager123     | Manager    | Escale Petroleum |
| cashier     | cashier123     | Cashier    | Escale Petroleum |
| inventory   | inventory123   | Inventory  | Escale Petroleum |
| supervisor  | supervisor123  | Supervisor | Escale Petroleum |

## Project Structure

```
Escale.API/
├── Controllers/           # 15 API controllers
├── Domain/
│   ├── Entities/          # 18 entity classes (BaseEntity/TenantEntity)
│   ├── Enums/             # UserRole, PaymentMethod, TransactionStatus, etc.
│   └── Constants/         # BusinessRules (VAT, thresholds, defaults)
├── Data/
│   ├── EscaleDbContext.cs # DbContext with soft delete + audit
│   ├── Configurations/    # EF Core fluent configurations
│   ├── Repositories/      # Generic repository + Unit of Work
│   └── Seeds/             # DatabaseSeeder (3 orgs, users, stations, etc.)
├── DTOs/                  # Request/response DTOs per domain
├── Services/
│   ├── Interfaces/        # 20 service contracts
│   └── Implementations/   # 20 service implementations
├── Mapping/               # AutoMapper MappingProfile
├── Middleware/             # ExceptionHandlingMiddleware
├── Extensions/            # JWT setup, DI registration, claims helpers
├── Hubs/                  # SignalR EscaleHub + NotificationConstants
├── Validators/            # FluentValidation rules (18 validators)
├── Migrations/            # EF Core migrations
└── Program.cs             # App configuration & startup
```

## Architecture

### Multi-Tenancy

All shared data extends `TenantEntity` which includes an `OrganizationId` foreign key. EF Core global query filters ensure data isolation between organizations. The `OrganizationId` is extracted from JWT claims and auto-set on entity creation.

SuperAdmin users can override tenant scoping via the `X-Organization-Id` header for cross-org operations.

### Entity Hierarchy

```
BaseEntity (Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted, DeletedAt)
├── TenantEntity (+ OrganizationId)
│   ├── User, Station, FuelType, Customer, Transaction
│   ├── Subscription, Shift, InventoryItem, OrganizationSettings
│   └── (scoped to organization)
└── Organization, Car, RefillRecord, FuelPrice, RefreshToken (global)
```

### Repository + Unit of Work

- `IRepository<T>` — generic CRUD with `Query()` for LINQ
- `IUnitOfWork` — exposes all repositories + `SaveChangesAsync()`

### Response Format

All responses use a consistent wrapper:

```json
{
  "Success": true,
  "Message": "Success",
  "Data": { }
}
```

Paginated responses:

```json
{
  "Success": true,
  "Data": {
    "Items": [],
    "TotalCount": 100,
    "Page": 1,
    "PageSize": 20,
    "TotalPages": 5,
    "HasPreviousPage": false,
    "HasNextPage": true
  }
}
```

Error responses:

```json
{
  "Success": false,
  "Message": "Validation failed",
  "Errors": ["Username is required"]
}
```

## API Endpoints

### Auth (`/api/auth`)

| Method | Route                | Auth     | Description              |
|--------|----------------------|----------|--------------------------|
| POST   | `/login`             | Public   | Login, returns JWT       |
| POST   | `/register`          | Public   | Register organization    |
| POST   | `/refresh-token`     | Public   | Refresh access token     |
| POST   | `/revoke-token`      | Required | Revoke refresh token     |
| GET    | `/profile`           | Required | Get current user profile |
| PUT    | `/profile`           | Required | Update profile           |
| POST   | `/change-password`   | Required | Change own password      |

### Sales (`/api/sales`)

| Method | Route     | Auth     | Description        |
|--------|-----------|----------|--------------------|
| POST   | `/`       | Required | Create a new sale  |
| GET    | `/recent` | Required | Get recent sales   |

### Dashboard (`/api/dashboard`)

| Method | Route                  | Auth     | Description             |
|--------|------------------------|----------|-------------------------|
| GET    | `/summary`             | Required | Dashboard summary stats |
| GET    | `/station-performance` | Required | Top station rankings    |

### Fuel Types (`/api/fueltypes`)

| Method | Route   | Auth  | Description       |
|--------|---------|-------|-------------------|
| GET    | `/`     | Any   | List fuel types   |
| GET    | `/{id}` | Any   | Get fuel type     |
| POST   | `/`     | Admin | Create fuel type  |
| PUT    | `/{id}` | Admin | Update fuel type  |
| DELETE | `/{id}` | Admin | Delete fuel type  |

### Stations (`/api/stations`)

| Method | Route   | Auth       | Description     |
|--------|---------|------------|-----------------|
| GET    | `/`     | Required   | List stations   |
| GET    | `/{id}` | Required   | Get station     |
| POST   | `/`     | SuperAdmin | Create station  |
| PUT    | `/{id}` | SuperAdmin | Update station  |
| DELETE | `/{id}` | SuperAdmin | Delete station  |

### Users (`/api/users`)

| Method | Route                   | Auth              | Description     |
|--------|-------------------------|-------------------|-----------------|
| GET    | `/`                     | Admin/Manager/SA  | List users      |
| GET    | `/{id}`                 | Admin/Manager/SA  | Get user        |
| POST   | `/`                     | Admin/Manager/SA  | Create user     |
| PUT    | `/{id}`                 | Admin/Manager/SA  | Update user     |
| DELETE | `/{id}`                 | Admin/Manager/SA  | Delete user     |
| POST   | `/{id}/change-password` | Admin/Manager/SA  | Reset password  |
| POST   | `/{id}/toggle-status`   | Admin/Manager/SA  | Toggle active   |

### Customers (`/api/customers`)

| Method | Route                                    | Auth          | Description             |
|--------|------------------------------------------|---------------|-------------------------|
| GET    | `/`                                      | Admin/Manager | List customers          |
| GET    | `/search?term={term}`                    | Admin/Manager | Search customers        |
| GET    | `/{id}`                                  | Admin/Manager | Get customer            |
| POST   | `/`                                      | Admin/Manager | Create customer         |
| PUT    | `/{id}`                                  | Admin/Manager | Update customer         |
| DELETE | `/{id}`                                  | Admin/Manager | Delete customer         |
| POST   | `/{customerId}/cars`                     | Admin/Manager | Add car                 |
| PUT    | `/{customerId}/cars/{carId}`             | Admin/Manager | Update car              |
| DELETE | `/{customerId}/cars/{carId}`             | Admin/Manager | Deactivate car          |
| POST   | `/{customerId}/cars/{carId}/reactivate`  | Admin/Manager | Reactivate car          |
| GET    | `/{customerId}/transactions`             | Admin/Manager | Customer transactions   |

### Subscriptions (`/api/subscriptions`)

| Method | Route                          | Auth              | Description          |
|--------|--------------------------------|-------------------|----------------------|
| POST   | `/topup`                       | Admin/Manager/SA  | Top up subscription  |
| GET    | `/customer/{id}/active`        | Admin/Manager     | Active subscription  |
| GET    | `/customer/{id}/history`       | Admin/Manager     | Subscription history |
| POST   | `/lookup`                      | Public            | Lookup by car        |
| POST   | `/{id}/cancel`                 | Admin/Manager/SA  | Cancel subscription  |

### Transactions (`/api/transactions`)

| Method | Route   | Auth     | Description                   |
|--------|---------|----------|-------------------------------|
| GET    | `/`     | Required | List (paginated, filterable)  |
| GET    | `/{id}` | Required | Get transaction details       |

### Inventory (`/api/inventory`)

| Method | Route            | Auth                    | Description          |
|--------|------------------|-------------------------|----------------------|
| GET    | `/`              | Required                | Get inventory        |
| GET    | `/refills`       | Required                | Recent refills       |
| POST   | `/refill`        | Admin/Manager/Inventory | Record refill        |
| PUT    | `/reorder-level` | Admin/Manager/Inventory | Update reorder level |

### Stock (`/api/stock`)

| Method | Route    | Auth                    | Description     |
|--------|----------|-------------------------|-----------------|
| GET    | `/`      | Required                | Stock levels    |
| POST   | `/refill`| Admin/Manager/Inventory | Record refill   |

### Shifts (`/api/shifts`)

| Method | Route      | Auth     | Description    |
|--------|------------|----------|----------------|
| GET    | `/current` | Required | Current shift  |
| POST   | `/clock`   | Required | Clock in/out   |
| GET    | `/summary` | Required | Shift summary  |

### Reports (`/api/reports`)

| Method | Route                   | Auth                    | Description        |
|--------|-------------------------|-------------------------|--------------------|
| GET    | `/sales`                | Admin/Manager/Inventory | Sales report       |
| GET    | `/inventory`            | Admin/Manager/Inventory | Inventory report   |
| GET    | `/employees`            | Admin/Manager/Inventory | Employee report    |
| GET    | `/customers`            | Admin/Manager/Inventory | Customer report    |
| GET    | `/financial`            | Admin/Manager/Inventory | Financial report   |
| GET    | `/transactions/export`  | Admin/Manager/Inventory | CSV export         |

### Settings (`/api/settings`)

| Method | Route          | Auth         | Description          |
|--------|----------------|--------------|----------------------|
| GET    | `/`            | Admin/SA     | Get settings         |
| PUT    | `/`            | Admin/SA     | Update settings      |
| GET    | `/ebm/status`  | Admin/SA     | EBM status           |
| POST   | `/ebm/sync`    | Admin/SA     | Sync with EBM        |
| GET    | `/ebm/config`  | Admin/SA     | EBM configuration    |
| POST   | `/ebm/test`    | Admin/SA     | Test EBM connection  |

### SuperAdmin (`/api/superadmin`)

| Method | Route                                              | Auth       | Description              |
|--------|------------------------------------------------------|------------|--------------------------|
| GET    | `/organizations`                                     | SuperAdmin | List organizations       |
| GET    | `/organizations/{id}`                                | SuperAdmin | Get organization         |
| POST   | `/organizations`                                     | SuperAdmin | Create organization      |
| PUT    | `/organizations/{id}`                                | SuperAdmin | Update organization      |
| DELETE | `/organizations/{id}`                                | SuperAdmin | Delete organization      |
| POST   | `/organizations/{id}/restore`                        | SuperAdmin | Restore organization     |
| GET    | `/organizations/{orgId}/stations`                    | SuperAdmin | Organization stations    |
| POST   | `/organizations/{orgId}/stations`                    | SuperAdmin | Create station           |
| POST   | `/organizations/{orgId}/stations/{id}/toggle-status` | SuperAdmin | Toggle station status    |
| GET    | `/organizations/{orgId}/settings/ebm`                | SuperAdmin | Get EBM config           |
| PUT    | `/organizations/{orgId}/settings/ebm`                | SuperAdmin | Configure EBM            |
| POST   | `/organizations/{orgId}/settings/ebm/test`           | SuperAdmin | Test EBM connection      |
| GET    | `/organizations/{orgId}/fueltypes`                   | SuperAdmin | Get fuel types           |
| POST   | `/organizations/{orgId}/fueltypes`                   | SuperAdmin | Create fuel type         |
| PUT    | `/organizations/{orgId}/fueltypes/{id}`              | SuperAdmin | Update fuel type         |
| DELETE | `/organizations/{orgId}/fueltypes/{id}`              | SuperAdmin | Delete fuel type         |
| GET    | `/organizations/{orgId}/fueltypes/deleted`           | SuperAdmin | Deleted fuel types       |
| POST   | `/organizations/{orgId}/fueltypes/{id}/restore`      | SuperAdmin | Restore fuel type        |
| GET    | `/organizations/{orgId}/admin`                       | SuperAdmin | Get org admin            |
| POST   | `/organizations/{orgId}/admin`                       | SuperAdmin | Create org admin         |

### Health Check

| Method | Route     | Auth   | Description |
|--------|-----------|--------|-------------|
| GET    | `/health` | Public | Returns "ok"|

## Enums

| Enum               | Values                                                       |
|--------------------|--------------------------------------------------------------|
| UserRole           | Admin, Manager, Cashier, Inventory, Supervisor, SuperAdmin   |
| CustomerType       | Individual, Corporate                                        |
| PaymentMethod      | Cash, MobileMoney, Card, Credit                              |
| TransactionStatus  | Completed, Voided, Pending                                   |
| SubscriptionStatus | Active, Inactive, Expired, Cancelled                         |
| StockStatus        | Normal, Low, Critical                                        |

## Business Rules

| Rule                    | Value              |
|-------------------------|--------------------|
| VAT Rate                | 18%                |
| Currency                | RWF                |
| Low Stock Threshold     | 25%                |
| Critical Stock Threshold| 10%                |
| Min Sale Amount         | 1,000 RWF          |
| Max Sale Amount         | 10,000,000 RWF     |
| Receipt Format          | RCP{yyyyMMddHHmmss}|
| Access Token Expiry     | 60 minutes         |
| Refresh Token Expiry    | 7 days             |

## SignalR Hub

**Endpoint:** `/hubs/escale`
**Auth:** JWT required (via query string `?access_token=`)

Clients are auto-grouped by `org_{OrganizationId}` for tenant-isolated notifications.

**Notification Types:**
- `fuel_types_changed`
- `sale_completed`
- `inventory_changed`
- `user_changed`
- `station_changed`
- `settings_changed`

## EBM Integration

Electronic Billing Machine integration for Rwanda tax compliance:

- Sale receipt submission with customer TIN
- Fuel price synchronization
- Stock level updates
- Product registration
- Connection testing

Configured per organization via `OrganizationSettings` (EBM server URL, business ID, branch ID, TIN, etc.).

## Logging

Serilog with two sinks:
- **Console** — all log output
- **File** — daily rotating logs in `Logs/escale-{date}.log` (30 day retention)

Log levels:
- Default: Information
- Microsoft/EF Core: Warning (Information in Development for SQL queries)
