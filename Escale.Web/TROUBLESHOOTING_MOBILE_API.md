# How to Fix the Timeout Error

## Problem
The mobile app shows: "The request was canceled due to the configured HttpClient.Timeout of 30 seconds elapsing"

This happens because the mobile app cannot connect to the API server.

## Solution Steps

### Step 1: Start the API Server

1. **In Visual Studio:**
   - Right-click on **Escale.API** project
   - Select "Set as Startup Project"
   - Press F5 or click the green play button
   - Wait for the API to start (you should see a browser open with Swagger UI at https://localhost:7015/swagger)

   **OR**

2. **From Command Line:**
   ```powershell
   cd "C:\Users\cyifu\Desktop\project\GasManagement\Escale.API"
   dotnet run
   ```

### Step 2: Verify API is Running

1. Open a browser and navigate to: https://localhost:7015/swagger
2. You should see the Swagger UI with available endpoints
3. Test the AuthController > POST /api/auth/login endpoint

### Step 3: Run the Mobile App

1. **Set Escale.mobile as startup project**
2. Choose your target platform:
   - **Android Emulator**: Will automatically use https://10.0.2.2:7015/api
   - **Windows**: Will use https://localhost:7015/api
   - **iOS Simulator**: Will use https://localhost:7015/api

### Step 4: Login

Use the demo credentials:
- **Username**: `admin`
- **Password**: `admin123`

## Common Issues and Solutions

### Issue 1: "Connection timeout"
**Solution:** Make sure the API is running. Check the Output window for errors.

### Issue 2: "SSL certificate error" (Android)
**Solution:** Already handled - the app bypasses SSL validation in DEBUG mode.

### Issue 3: "Cannot connect to API"
**Solution:** 
- Verify the API is listening on port 7015
- Check Windows Firewall settings
- Make sure antivirus isn't blocking the connection

### Issue 4: Running both projects simultaneously

**Option A: Multiple Startup Projects**
1. Right-click on Solution in Solution Explorer
2. Select "Configure Startup Projects"
3. Choose "Multiple startup projects"
4. Set both **Escale.API** and **Escale.mobile** to "Start"

**Option B: Keep API Running**
1. Start Escale.API first (Ctrl+F5 for "Start Without Debugging")
2. Then start Escale.mobile from another Visual Studio instance or change startup project

## API Configuration

The API URLs are configured in `ApiService.cs`:
- **Android Emulator**: `https://10.0.2.2:7015/api`
- **iOS/Windows**: `https://localhost:7015/api`

To change the port, update:
1. `Escale.API/Properties/launchSettings.json`
2. `Escale.mobile/Services/ApiService.cs` - GetBaseUrl() method

## Testing the API Manually

Open Swagger UI at: https://localhost:7015/swagger

Test the login endpoint:
1. Click on "POST /api/auth/login"
2. Click "Try it out"
3. Enter:
   ```json
   {
     "username": "admin",
     "password": "admin123",
     "rememberMe": false
   }
   ```
4. Click "Execute"
5. You should get a 200 response with a token

## Debugging Tips

The mobile app now includes debug logging. Check the Output window for messages like:
- "API Base URL: ..."
- "Attempting login to: ..."
- "Response status: ..."

If you see timeouts, the API isn't reachable from your device/emulator.

## For Android Emulator Users

The Android emulator uses a special IP address to access the host machine:
- **10.0.2.2** = Your computer's localhost
- **127.0.0.1** = The emulator itself (won't work!)

The app is already configured to use 10.0.2.2 for Android.

## Next Steps

Once login works:
1. Implement real database authentication
2. Add JWT token validation
3. Create other API endpoints (dashboard, sales, etc.)
4. Add proper user management
