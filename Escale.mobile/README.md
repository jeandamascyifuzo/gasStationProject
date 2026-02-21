# Escale Mobile Cashier App

## Overview
A .NET MAUI mobile application for gas station cashiers to manage fuel sales, check stock levels, and process transactions with EBM (Electronic Billing Machine) integration.

## Features

### 1. Authentication
- Login with username and password
- Remember me functionality
- Secure credential storage
- Station selection for multi-station users

### 2. Dashboard
- Today's sales summary
- Transaction count
- Low stock alerts
- Quick actions for new sales and stock checks

### 3. New Sale Flow
**Step 1: Fuel & Payment Selection**
- Select fuel type (Petrol, Diesel, Kerosene)
- Enter amount in RWF or liters (auto-calculates)
- Choose payment method (Cash, Mobile Money, Card, Credit)

**Step 2a: Walk-in Customer (Optional)**
- Enter customer name and phone number
- Skip to continue as guest

**Step 2b: Credit Customer Search**
- Search by plate number or car PIN
- View customer credit limits
- Select customer from results

**Step 3: Preview**
- Review sale details
- View calculated totals with VAT
- Edit or confirm sale

**Step 4: Complete**
- EBM invoice generation
- Receipt number
- Print receipt option
- Quick new sale or return to dashboard

### 4. Stock Management
- View current fuel levels
- Percentage remaining indicators
- Low stock warnings
- Request reorder functionality

### 5. Transactions
- Today's transaction list
- View transaction details
- Filter by date
- EBM status indicators

### 6. Profile
- User information
- Clock in/out functionality
- Shift summary
- Change password
- Logout

## Architecture

### Project Structure
```
Escale.mobile/
??? Models/              # Data models
??? ViewModels/          # MVVM ViewModels
??? Views/               # XAML pages
??? Services/            # API and app state services
??? Converters/          # Value converters
??? Resources/           # Images, fonts, styles
```

### Key Technologies
- **.NET MAUI 9.0** - Cross-platform UI framework
- **CommunityToolkit.Mvvm** - MVVM helpers
- **Dependency Injection** - Service registration
- **Async/Await** - Asynchronous operations

### Services

**ApiService**
- Handles all HTTP API calls
- Authentication token management
- Error handling

**AppState**
- Global application state
- User session management
- Current sale tracking
- State change notifications

## Setup

### Prerequisites
- Visual Studio 2022 17.8 or later
- .NET 9.0 SDK
- Android SDK (for Android)
- Xcode (for iOS/macOS)

### Configuration

1. Update API URL in `Services/ApiService.cs`:
```csharp
private readonly string _baseUrl = "https://your-api-url.com/api";
```

2. Build and run:
```bash
dotnet build
dotnet run
```

## API Integration

### Required API Endpoints

**Authentication**
- POST `/api/auth/login`

**Dashboard**
- GET `/api/dashboard/summary?stationId={id}`

**Fuel Types**
- GET `/api/fueltypes?stationId={id}`

**Customers**
- GET `/api/customers/search?term={term}`

**Sales**
- POST `/api/sales`

**Transactions**
- GET `/api/transactions?stationId={id}&date={date}`

**Stock**
- GET `/api/stock?stationId={id}`

**Shifts**
- GET `/api/shifts/current?userId={id}&stationId={id}`
- POST `/api/shifts/clock`

## UI/UX Guidelines

### Design Principles
- **Mobile-First**: Optimized for touch interactions
- **Large Touch Targets**: Minimum 44x44 points
- **Clear Visual Hierarchy**: Important actions prominent
- **Immediate Feedback**: Loading states and confirmation messages

### Color Scheme
- Primary: #1E3A8A (Blue)
- Success: #10B981 (Green)
- Warning: #F59E0B (Orange)
- Danger: #EF4444 (Red)
- Background: #F8F9FA (Light Gray)

### Typography
- Headings: OpenSans-Semibold
- Body: OpenSans-Regular
- Sizes: 12-28 points based on hierarchy

## Features by Priority

### Phase 1 (Complete)
? Login & Station Selection
? Dashboard with bottom navigation
? New Sale flow (walk-in)
? Credit customer search
? Preview & invoice
? Transactions list
? Stock view
? Profile management

### Phase 2 (Future)
- Offline mode with sync
- Bluetooth printer integration
- Receipt templates
- Report generation
- Multi-language support
- Biometric authentication

## Testing

### Manual Testing Checklist
- [ ] Login with valid/invalid credentials
- [ ] Station selection flow
- [ ] Complete walk-in sale
- [ ] Complete credit sale
- [ ] View stock levels
- [ ] Browse transactions
- [ ] Clock in/out
- [ ] Logout and re-login

### Test Accounts
```
Username: cashier1
Password: test123

Username: admin
Password: admin123
```

## Troubleshooting

### Common Issues

**Build Errors**
- Ensure .NET 9.0 SDK is installed
- Clean and rebuild solution
- Check package versions

**API Connection Failed**
- Verify API URL is correct
- Check network connectivity
- Ensure API is running

**Navigation Issues**
- Verify routes are registered in AppShell
- Check ViewModel dependencies

## Support

For issues and questions:
- Email: support@escale.com
- GitHub: https://github.com/jeandamascyifuzo/gasStationProject

## License
Copyright © 2024 Escale Gas Station Management
