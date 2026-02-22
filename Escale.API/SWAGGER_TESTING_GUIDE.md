# Escale API - Swagger Testing Guide

## 1. Start the API

```bash
cd Escale.API
dotnet run --launch-profile https
```

Open your browser at: **https://localhost:7015**

Swagger UI loads automatically at the root URL.

---

## 2. Login (Get your JWT Token)

All endpoints except Login and Register require a JWT token.

1. Expand **Auth > POST /api/Auth/login**
2. Click **Try it out**
3. Paste this body:

```json
{
  "Username": "admin",
  "Password": "admin123",
  "RememberMe": false
}
```

4. Click **Execute**
5. From the response, **copy the `Token` value** (the long string starting with `eyJ...`)

Other test accounts:
| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Admin (full access) |
| manager | manager123 | Manager |
| cashier | cashier123 | Cashier |

---

## 3. Authorize Swagger

1. Click the **Authorize** button (lock icon at the top right)
2. In the **Value** field, type: `Bearer <paste-your-token-here>`
   - Example: `Bearer eyJhbGciOiJIUzI1NiIs...`
3. Click **Authorize**, then **Close**

Now all endpoints will include your token automatically.

---

## 4. Test Each Endpoint

### 4.1 Auth

| Method | Endpoint | Body | Notes |
|--------|----------|------|-------|
| POST | `/api/Auth/login` | `{"Username":"admin","Password":"admin123","RememberMe":false}` | No auth needed |
| POST | `/api/Auth/register` | See below | No auth needed, creates new org |
| POST | `/api/Auth/refresh-token` | `{"RefreshToken":"<token-from-login>"}` | No auth needed |
| POST | `/api/Auth/revoke-token` | `{"RefreshToken":"<token-to-revoke>"}` | Requires auth |

Register body:
```json
{
  "OrganizationName": "Test Gas Station",
  "AdminUsername": "testadmin",
  "AdminPassword": "test123",
  "AdminFullName": "Test Admin",
  "AdminEmail": "test@test.com",
  "Phone": "+250788999999",
  "TIN": "987654321",
  "Address": "Test Address"
}
```

---

### 4.2 Stations (Login as Admin)

| Method | Endpoint | Notes |
|--------|----------|-------|
| GET | `/api/Stations` | Lists all stations |
| GET | `/api/Stations/{id}` | Copy an Id from the list above |
| POST | `/api/Stations` | See body below |
| PUT | `/api/Stations/{id}` | Use an Id from GET |
| DELETE | `/api/Stations/{id}` | Use an Id from GET |

Create body:
```json
{
  "Name": "South Station",
  "Location": "Nyanza",
  "Address": "KK 20 Rd",
  "PhoneNumber": "+250788000003"
}
```

---

### 4.3 Fuel Types (Login as Admin)

| Method | Endpoint | Notes |
|--------|----------|-------|
| GET | `/api/FuelTypes` | Lists all 4 seeded fuel types |
| GET | `/api/FuelTypes/{id}` | Copy Id from the list |
| POST | `/api/FuelTypes` | See body below |
| PUT | `/api/FuelTypes/{id}` | Update name/price |
| DELETE | `/api/FuelTypes/{id}` | Soft deletes |

Create body:
```json
{
  "Name": "Premium Diesel",
  "PricePerLiter": 1500
}
```

---

### 4.4 Users (Login as Admin)

| Method | Endpoint | Notes |
|--------|----------|-------|
| GET | `/api/Users` | Paginated. Optional: `?Page=1&PageSize=20` |
| GET | `/api/Users/{id}` | Copy Id from list |
| POST | `/api/Users` | See body below |
| PUT | `/api/Users/{id}` | Update user |
| DELETE | `/api/Users/{id}` | Soft deletes |
| POST | `/api/Users/{id}/change-password` | See body below |
| POST | `/api/Users/{id}/toggle-status` | Enables/disables user |

Create user body (use StationIds from GET /api/Stations):
```json
{
  "Username": "newcashier",
  "Password": "pass123",
  "FullName": "New Cashier",
  "Email": "new@escale.rw",
  "Phone": "+250788111111",
  "Role": "Cashier",
  "StationIds": ["<paste-station-id-here>"]
}
```

Change password body:
```json
{
  "CurrentPassword": "pass123",
  "NewPassword": "newpass123"
}
```

---

### 4.5 Customers

| Method | Endpoint | Notes |
|--------|----------|-------|
| GET | `/api/Customers` | Paginated. Optional: `?SearchTerm=john` |
| GET | `/api/Customers/search?term=john` | Quick search |
| GET | `/api/Customers/{id}` | Get with cars & subscriptions |
| POST | `/api/Customers` | See body below |
| PUT | `/api/Customers/{id}` | Update customer |
| DELETE | `/api/Customers/{id}` | Soft deletes |

Create body:
```json
{
  "Name": "ABC Transport Ltd",
  "PhoneNumber": "+250788222222",
  "Email": "abc@transport.rw",
  "Type": "Corporate",
  "TIN": "111222333",
  "CreditLimit": 5000000,
  "Cars": [
    {
      "PlateNumber": "RAD 123A",
      "Make": "Toyota",
      "Model": "Hilux",
      "Year": 2022
    }
  ]
}
```

---

### 4.6 Shifts (Clock In first before making Sales)

| Method | Endpoint | Notes |
|--------|----------|-------|
| GET | `/api/Shifts/current?userId={id}&stationId={id}` | Get active shift |
| POST | `/api/Shifts/clock` | Clock in/out |
| GET | `/api/Shifts/summary?userId={id}&stationId={id}` | Shift stats |

**Step 1: Clock in** (use UserId and StationId from earlier calls):
```json
{
  "UserId": "<your-user-id>",
  "StationId": "<station-id>",
  "IsClockIn": true
}
```

**Step 2: Clock out** (same body but `IsClockIn: false`):
```json
{
  "UserId": "<your-user-id>",
  "StationId": "<station-id>",
  "IsClockIn": false
}
```

---

### 4.7 Sales (Main workflow)

| Method | Endpoint | Notes |
|--------|----------|-------|
| POST | `/api/Sales` | Create a sale |
| GET | `/api/Sales/recent?count=10` | Recent sales. Optional: `&stationId={id}` |

Create sale body (use StationId from Stations, FuelType name from FuelTypes):
```json
{
  "StationId": "<station-id>",
  "FuelType": "Petrol 95",
  "Liters": 25.5,
  "PricePerLiter": 1450,
  "PaymentMethod": "Cash",
  "Customer": {
    "Name": "Walk-in Customer",
    "PhoneNumber": "+250788333333"
  }
}
```

Payment methods: `Cash`, `MobileMoney`, `Card`, `Credit`

---

### 4.8 Transactions

| Method | Endpoint | Notes |
|--------|----------|-------|
| GET | `/api/Transactions` | Filtered list. Params: `?StationId=&StartDate=2026-01-01&EndDate=2026-12-31&Page=1&PageSize=50` |
| GET | `/api/Transactions/{id}` | Get single transaction |

---

### 4.9 Stock

| Method | Endpoint | Notes |
|--------|----------|-------|
| GET | `/api/Stock?stationId={id}` | Stock levels (stationId optional) |
| POST | `/api/Stock/refill` | Record refill (Admin/Manager) |

Refill body:
```json
{
  "StationId": "<station-id>",
  "FuelType": "Diesel",
  "Quantity": 5000,
  "UnitCost": 1200,
  "SupplierName": "Rwanda Fuel Ltd",
  "InvoiceNumber": "INV-2026-001",
  "RefillDate": "2026-02-22T10:00:00"
}
```

---

### 4.10 Inventory (Admin/Manager for writes)

| Method | Endpoint | Notes |
|--------|----------|-------|
| GET | `/api/Inventory?stationId={id}` | All inventory items |
| GET | `/api/Inventory/refills?count=20` | Refill history |
| POST | `/api/Inventory/refill` | Record refill |
| PUT | `/api/Inventory/reorder-level` | Update reorder threshold |

Refill body (use InventoryItemId from GET /api/Inventory):
```json
{
  "InventoryItemId": "<inventory-item-id>",
  "Quantity": 3000,
  "UnitCost": 1100,
  "SupplierName": "Total Rwanda",
  "InvoiceNumber": "INV-2026-002",
  "RefillDate": "2026-02-22T10:00:00"
}
```

Update reorder level:
```json
{
  "Id": "<inventory-item-id>",
  "ReorderLevel": 4000
}
```

---

### 4.11 Dashboard

| Method | Endpoint | Notes |
|--------|----------|-------|
| GET | `/api/Dashboard/summary` | Overall dashboard |
| GET | `/api/Dashboard/summary?stationId={id}` | Per-station dashboard |
| GET | `/api/Dashboard/summary?date=2026-02-22` | Specific date |

---

### 4.12 Reports (Admin/Manager only)

| Method | Endpoint | Notes |
|--------|----------|-------|
| GET | `/api/Reports/sales?startDate=2026-01-01&endDate=2026-12-31` | Sales report |
| GET | `/api/Reports/inventory` | Inventory report |
| GET | `/api/Reports/employees?startDate=2026-01-01&endDate=2026-12-31` | Employee report |
| GET | `/api/Reports/customers?startDate=2026-01-01&endDate=2026-12-31` | Customer report |
| GET | `/api/Reports/financial?startDate=2026-01-01&endDate=2026-12-31` | Financial report |
| GET | `/api/Reports/transactions/export?startDate=2026-01-01&endDate=2026-12-31` | CSV download |

---

### 4.13 Settings (Admin only)

| Method | Endpoint | Notes |
|--------|----------|-------|
| GET | `/api/Settings` | Current settings |
| PUT | `/api/Settings` | Update settings (copy response from GET, modify, send back) |
| GET | `/api/Settings/ebm/status` | EBM connection status |
| POST | `/api/Settings/ebm/sync` | Trigger EBM sync |

---

## 5. Recommended Testing Flow

Follow this order for a complete test:

```
1. POST /api/Auth/login          --> Get token, authorize Swagger
2. GET  /api/Stations            --> Note a StationId
3. GET  /api/FuelTypes           --> Note fuel type names
4. GET  /api/Users               --> Note your UserId
5. POST /api/Shifts/clock        --> Clock in (IsClockIn: true)
6. POST /api/Customers           --> Create a customer
7. POST /api/Sales               --> Create a sale
8. POST /api/Sales               --> Create 2-3 more sales
9. GET  /api/Sales/recent        --> Verify sales appear
10. GET /api/Transactions        --> Verify transactions listed
11. GET /api/Stock               --> Check stock decreased
12. GET /api/Dashboard/summary   --> See today's stats
13. GET /api/Reports/sales       --> See sales breakdown
14. POST /api/Shifts/clock       --> Clock out (IsClockIn: false)
15. GET /api/Shifts/summary      --> See shift stats
```

---

## 6. Multi-Tenancy Test

To verify data isolation between organizations:

1. **POST /api/Auth/register** with a new organization name
2. Copy the new token from the response
3. Click **Authorize** in Swagger, clear the old token, paste the new one
4. **GET /api/Stations** --> Should return empty (new org has no stations)
5. **GET /api/FuelTypes** --> Should return 4 default fuel types (created during registration)
6. **GET /api/Users** --> Should return only the new admin user

The new organization cannot see data from the "Escale Gas Station" organization.

---

## 7. Role-Based Access Test

| Endpoint | Admin | Manager | Cashier |
|----------|-------|---------|---------|
| Users CRUD | Yes | 403 Forbidden | 403 |
| Settings | Yes | 403 | 403 |
| Reports | Yes | Yes | 403 |
| Inventory refill | Yes | Yes | 403 |
| Sales | Yes | Yes | Yes |
| Dashboard | Yes | Yes | Yes |
| Stations read | Yes | Yes | Yes |
| Stations write | Yes | 403 | 403 |

To test: Login as `cashier/cashier123`, then try `GET /api/Users` --> expect 403.

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| 401 Unauthorized | Token expired or missing. Login again and re-authorize |
| 403 Forbidden | Your role doesn't have access. Login with admin account |
| 500 Internal Server Error | Check the API console logs for details |
| Connection refused | Make sure the API is running (`dotnet run`) |
| Database error on startup | Make sure SQL Server is running and the connection string in `appsettings.json` is correct |
