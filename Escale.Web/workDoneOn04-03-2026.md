# Work Done — March 4, 2026

## Overview

Three major features implemented across API, Web, and Mobile projects, plus real-time dashboard updates and an organization creation fix.

---

## Part 1: API — Self-Service Profile & Change Password Endpoints

**Problem:** The existing `POST /api/users/{id}/change-password` on `UsersController` is restricted to `Admin,SuperAdmin`. Cashiers and Managers cannot change their own password.

**Solution:** Added self-service endpoints on `AuthController` with `[Authorize]` (any authenticated user).

### Files Modified
- `Escale.API/DTOs/Auth/AuthDtos.cs` — Added `ProfileResponseDto` and `UpdateProfileRequestDto`
- `Escale.API/Services/Interfaces/IAuthService.cs` — Added `GetProfileAsync`, `UpdateProfileAsync`, `ChangeOwnPasswordAsync`
- `Escale.API/Services/Implementations/AuthService.cs` — Implemented all 3 methods (BCrypt verification for password change)
- `Escale.API/Controllers/AuthController.cs` — Added 3 new endpoints:
  - `GET /api/auth/profile` — Get own profile
  - `PUT /api/auth/profile` — Update own FullName, Email, Phone
  - `POST /api/auth/change-password` — Change own password (requires current password)

---

## Part 2: API — SuperAdmin Create Organization Admin

**Problem:** SuperAdmin needs the ability to manually create an Admin user for each organization.

### Files Modified
- `Escale.API/DTOs/Organizations/OrganizationDtos.cs` — Added `CreateOrgAdminRequestDto`
- `Escale.API/Services/Interfaces/IOrganizationService.cs` — Added `GetOrganizationAdminAsync`, `CreateOrganizationAdminAsync`
- `Escale.API/Services/Implementations/OrganizationService.cs` — Implemented with validation:
  - Organization must exist
  - No existing Admin user in that org
  - Username must be unique
- `Escale.API/Controllers/SuperAdminController.cs` — Added 2 endpoints:
  - `GET /api/superadmin/organizations/{orgId}/admin`
  - `POST /api/superadmin/organizations/{orgId}/admin`

---

## Part 3: Mobile — Change Password Feature

**Problem:** The "Change Password" button on the Profile page showed a "Coming Soon" alert.

### Files Created
- `Escale.mobile/ViewModels/ChangePasswordViewModel.cs` — VM with validation (required fields, min length, passwords match, different from current), API call, logout on success
- `Escale.mobile/Views/ChangePasswordPage.xaml` — UI matching login page style (rounded card, secure entries, submit/cancel buttons)
- `Escale.mobile/Views/ChangePasswordPage.xaml.cs` — Code-behind with DI constructor

### Files Modified
- `Escale.mobile/Services/ApiService.cs` — Added `ChangePasswordAsync` method calling `POST /api/auth/change-password`
- `Escale.mobile/MauiProgram.cs` — Registered `ChangePasswordViewModel` and `ChangePasswordPage` in DI
- `Escale.mobile/ViewModels/ProfileViewModel.cs` — Replaced "Coming Soon" stub with navigation to `ChangePasswordPage`

---

## Part 4: Web — Profile Page

**Problem:** "My Profile" link in the top navigation dropdown pointed to `#` (dead link). No profile management existed.

### Files Created
- `Escale.Web/Models/Api/ProfileDtos.cs` — `ProfileResponseDto`, `UpdateProfileRequestDto`
- `Escale.Web/Controllers/ProfileController.cs` — Three actions:
  - `GET /Profile` — Fetch and display profile
  - `POST /Profile/Update` — Update profile info, update session display name
  - `POST /Profile/ChangePassword` — Change password, clear session/cookies, redirect to login
- `Escale.Web/Views/Profile/Index.cshtml` — Two Bootstrap cards:
  - Profile info edit form (FullName, Email, Phone; Username and Role are readonly)
  - Change password form (Current, New, Confirm) with logout warning

### Files Modified
- `Escale.Web/Services/Interfaces/IApiAuthService.cs` — Added `GetProfileAsync`, `UpdateProfileAsync`, `ChangePasswordAsync`
- `Escale.Web/Services/Implementations/ApiAuthService.cs` — Implemented all 3 methods with Bearer token auth
- `Escale.Web/Views/Shared/_TopNav.cshtml` — Fixed dead link: `href="#"` → `asp-controller="Profile" asp-action="Index"`

---

## Part 5: Web — SuperAdmin Create Admin UI

**Problem:** SuperAdmin had no way to create an admin user for an organization from the web interface.

### Files Modified
- `Escale.Web/Models/Api/OrganizationDtos.cs` — Added `CreateOrgAdminRequestDto`
- `Escale.Web/Models/OrganizationViewModel.cs` — Added `AdminUser` property to `OrganizationDetailsViewModel` and new `AdminUser` class
- `Escale.Web/Services/Interfaces/IApiOrganizationService.cs` — Added `GetAdminAsync`, `CreateAdminAsync`
- `Escale.Web/Services/Implementations/ApiOrganizationService.cs` — Implemented both methods
- `Escale.Web/Controllers/OrganizationsController.cs`:
  - `Details()` now fetches admin user in parallel with other data
  - Added `POST CreateAdmin` action
- `Escale.Web/Views/Organizations/Details.cshtml` — Added "Admin User" card:
  - Shows existing admin info (username, name, email, phone, status, last login) if one exists
  - Shows "Create Admin" button with modal form (username, password, full name, email, phone) if no admin exists

---

## Part 6: Real-Time Dashboard Updates via SignalR

**Problem:** When a cashier makes a sale on mobile, the web dashboard did not update until manually refreshed.

### Files Modified
- `Escale.Web/Views/Shared/_Layout.cshtml`:
  - Added SignalR client library via CDN (`signalr.min.js`)
  - Renders API hub URL and access token server-side into `escaleConfig` JS variable
- `Escale.Web/wwwroot/js/site.js`:
  - Added SignalR connection with JWT auth and auto-reconnect
  - Listens for `DataChanged` events
  - On `sale_completed`: shows toast notification and auto-refreshes Dashboard page
  - Also auto-refreshes Transactions page on `sale_completed` and Inventory page on `inventory_changed`

### How It Works
1. Cashier makes sale on mobile → API `SaleService` saves transaction
2. API fires `NotificationService.NotifyDataChangedAsync(orgId, "sale_completed")`
3. SignalR hub broadcasts to the organization group
4. Web browser receives event → shows toast → auto-reloads page with fresh data

---

## Part 7: Organization Creation Fix

**Problem:** When SuperAdmin created a new organization, an admin user was auto-created with a hardcoded password (`admin123`). SuperAdmin should create the admin manually.

### Files Modified
- `Escale.API/Services/Implementations/OrganizationService.cs`:
  - Removed the auto-created admin user block from `CreateOrganizationAsync()`
  - Updated `UserCount` in response from `1` to `0`

Now when a new organization is created, only the org, default fuel types, and default settings are created. The SuperAdmin then creates the admin from the Organization Details page.

---

## Summary of All New/Modified Files

### New Files (7)
| File | Description |
|------|-------------|
| `Escale.mobile/ViewModels/ChangePasswordViewModel.cs` | Mobile change password VM |
| `Escale.mobile/Views/ChangePasswordPage.xaml` | Mobile change password UI |
| `Escale.mobile/Views/ChangePasswordPage.xaml.cs` | Mobile change password code-behind |
| `Escale.Web/Models/Api/ProfileDtos.cs` | Web profile DTOs |
| `Escale.Web/Controllers/ProfileController.cs` | Web profile controller |
| `Escale.Web/Views/Profile/Index.cshtml` | Web profile page view |
| `Escale.Web/workDoneOn04-03-2026.md` | This report |

### Modified Files (20)
| File | Changes |
|------|---------|
| `Escale.API/DTOs/Auth/AuthDtos.cs` | Added ProfileResponseDto, UpdateProfileRequestDto |
| `Escale.API/DTOs/Organizations/OrganizationDtos.cs` | Added CreateOrgAdminRequestDto |
| `Escale.API/Services/Interfaces/IAuthService.cs` | Added 3 profile/password methods |
| `Escale.API/Services/Implementations/AuthService.cs` | Implemented profile/password methods |
| `Escale.API/Controllers/AuthController.cs` | Added 3 self-service endpoints |
| `Escale.API/Services/Interfaces/IOrganizationService.cs` | Added admin CRUD methods |
| `Escale.API/Services/Implementations/OrganizationService.cs` | Implemented admin CRUD, removed auto-admin |
| `Escale.API/Controllers/SuperAdminController.cs` | Added admin endpoints |
| `Escale.mobile/Services/ApiService.cs` | Added ChangePasswordAsync |
| `Escale.mobile/ViewModels/ProfileViewModel.cs` | Wired up change password navigation |
| `Escale.mobile/MauiProgram.cs` | Registered new VM and page |
| `Escale.Web/Services/Interfaces/IApiAuthService.cs` | Added profile/password methods |
| `Escale.Web/Services/Implementations/ApiAuthService.cs` | Implemented profile/password methods |
| `Escale.Web/Services/Interfaces/IApiOrganizationService.cs` | Added admin methods |
| `Escale.Web/Services/Implementations/ApiOrganizationService.cs` | Implemented admin methods |
| `Escale.Web/Models/OrganizationViewModel.cs` | Added AdminUser property |
| `Escale.Web/Models/Api/OrganizationDtos.cs` | Added CreateOrgAdminRequestDto |
| `Escale.Web/Controllers/OrganizationsController.cs` | Fetch admin in Details, CreateAdmin action |
| `Escale.Web/Views/Organizations/Details.cshtml` | Admin user card + create modal |
| `Escale.Web/Views/Shared/_TopNav.cshtml` | Fixed My Profile link |
| `Escale.Web/Views/Shared/_Layout.cshtml` | Added SignalR client + config |
| `Escale.Web/wwwroot/js/site.js` | Added SignalR real-time refresh logic |
