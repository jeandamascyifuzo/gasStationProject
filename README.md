# Escale Gas Station Management System

A multi-tenant SaaS platform for managing gas station operations — fuel sales, inventory tracking, customer subscriptions, employee shifts, reporting, and EBM (Electronic Billing Machine) integration for Rwanda tax compliance.

## Projects

| Project | Type | Port | Description |
|---------|------|------|-------------|
| [Escale.API](./Escale.API/) | .NET 8 Web API | 7015 | REST API backend with JWT auth, SignalR, EF Core |
| [Escale.Web](./Escale.Web/) | ASP.NET Core MVC | 7025 | Admin/management web portal |
| [Escale.mobile](./Escale.mobile/) | .NET 9 MAUI | — | Cashier mobile app (Android/iOS) |

## Architecture

```
┌─────────────────────┐     ┌──────────────────────┐
│   Escale.mobile     │     │    Escale.Web         │
│   (MAUI Cashier)    │     │    (Admin Portal)     │
│   Android/iOS       │     │    Razor + Bootstrap  │
└────────┬────────────┘     └────────┬─────────────┘
         │  HTTPS + SignalR          │  HTTPS + SignalR
         └──────────┬────────────────┘
                    │
          ┌─────────▼─────────┐
          │    Escale.API      │
          │  .NET 8 Web API    │
          │  JWT + EF Core     │
          └─────────┬─────────┘
                    │
          ┌─────────▼─────────┐
          │   SQL Server       │
          │   (EscaleDb)       │
          └───────────────────┘
```

### Key Patterns

- **Multi-Tenancy** — Organizations are isolated via `OrganizationId` on all data entities
- **Repository + Unit of Work** — Data access abstraction
- **JWT Authentication** — Access tokens (60 min) + refresh tokens (7 days)
- **SignalR** — Real-time notifications for sales, inventory, price changes
- **EBM Integration** — Rwanda Electronic Billing Machine for tax receipts
- **Soft Delete** — All entities support soft delete with audit trail

## Quick Start

### 1. Start the API

```bash
cd Escale.API
dotnet run
```

Database is created and seeded automatically. Swagger UI at `https://localhost:7015`.

### 2. Start the Web Portal

```bash
cd Escale.Web
dotnet run
```

Open `https://localhost:7025` and login.

### 3. Run the Mobile App

```bash
cd Escale.mobile
dotnet build -t:Run -f net9.0-android
```

### Test Accounts (Seeded)

| Username    | Password       | Role       | Organization     |
|-------------|----------------|------------|------------------|
| superadmin  | superadmin123  | SuperAdmin | System           |
| admin       | admin123       | Admin      | Escale Petroleum |
| manager     | manager123     | Manager    | Escale Petroleum |
| cashier     | cashier123     | Cashier    | Escale Petroleum |
| inventory   | inventory123   | Inventory  | Escale Petroleum |
| supervisor  | supervisor123  | Supervisor | Escale Petroleum |

## Roles

| Role        | Web Portal | Mobile App | Scope                                |
|-------------|------------|------------|--------------------------------------|
| SuperAdmin  | Yes        | No         | Manage all organizations             |
| Admin       | Yes        | Yes        | Full organization access             |
| Manager     | Yes        | Yes        | Station/department management        |
| Supervisor  | Yes        | No         | Assigned stations oversight          |
| Inventory   | Yes        | No         | Stock management and reports         |
| Cashier     | No         | Yes        | Process sales at assigned station    |

## Features

### Sales & Transactions
- Multi-step sale flow with fuel selection, customer info, and payment
- Payment methods: Cash, Mobile Money, Card, Credit
- VAT calculation (18%) and receipt generation
- EBM receipt submission for tax compliance
- Subscription-based customer support with balance tracking

### Inventory Management
- Real-time stock levels per station
- Refill recording with supplier tracking
- Low/critical stock alerts with configurable thresholds
- Reorder level management

### Customer Management
- Individual and corporate customers
- Vehicle registration with plate number and PIN
- Credit limits and balance tracking
- Subscription top-up and history

### Reporting
- Sales, inventory, employee, customer, and financial reports
- Date range and station filtering
- CSV export

### Dashboard
- Real-time sales statistics
- Top station rankings
- Live transaction feed (SignalR)
- Interactive charts (Chart.js)

## Tech Stack

| Component      | Technology                                    |
|----------------|-----------------------------------------------|
| API            | .NET 8, EF Core 8, SQL Server, JWT, SignalR   |
| Web            | ASP.NET Core MVC, Razor, Bootstrap 5, Chart.js|
| Mobile         | .NET MAUI 9, CommunityToolkit.Mvvm, SignalR   |
| Auth           | JWT (BCrypt password hashing)                 |
| Validation     | FluentValidation                              |
| Mapping        | AutoMapper                                    |
| Logging        | Serilog (console + file)                      |
| Real-time      | SignalR WebSocket hub                         |
| Tax            | EBM integration (Rwanda RRA)                  |
