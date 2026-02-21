# Quick Start Guide - Fixing Timeout Error

## THE PROBLEM
Your mobile app shows: **"Error: The request was canceled due to the configured HttpClient.Timeout of 30 seconds elapsing"**

This means the mobile app cannot reach the API server.

## THE SOLUTION (3 Simple Steps)

### ? Step 1: Start the API Server

**Option A - Using Visual Studio:**
1. In Solution Explorer, right-click **Escale.API** project
2. Click **"Debug" ? "Start New Instance"**
3. A browser window will open showing Swagger UI at `https://localhost:7015/swagger`
4. **Leave this running!**

**Option B - Using Command Line:**
```powershell
cd "C:\Users\cyifu\Desktop\project\GasManagement\Escale.API"
dotnet run
```

### ? Step 2: Verify API is Working

1. Open browser: https://localhost:7015/swagger
2. You should see "Escale.API" with a list of endpoints
3. Test the login endpoint:
   - Click **POST /api/auth/login**
   - Click **"Try it out"**
   - Use this JSON:
     ```json
     {
       "username": "admin",
       "password": "admin123",
       "rememberMe": false
     }
     ```
   - Click **Execute**
   - Should return: `"success": true` with a token

### ? Step 3: Run Mobile App

1. In Visual Studio, set **Escale.mobile** as startup project
2. Choose your device/emulator:
   - Android Emulator (recommended for testing)
   - Windows Machine
   - iOS Simulator
3. Press **F5** to run
4. Login with:
   - **Username:** `admin`
   - **Password:** `admin123`

## ?? IMPORTANT NOTES

- **Keep the API running** while using the mobile app
- The API must be on port **7015** (already configured)
- For Android Emulator, the app uses special address `10.0.2.2` to access your PC's localhost
- SSL certificate errors are automatically handled in DEBUG mode

## ?? If It Still Doesn't Work

### Check 1: Is the API actually running?
Look for this in Output window: `Now listening on: https://localhost:7015`

### Check 2: Firewall blocking?
- Temporarily disable Windows Firewall
- Or add exception for port 7015

### Check 3: Wrong URL?
The mobile app uses these URLs automatically:
- **Android:** `https://10.0.2.2:7015/api`
- **Windows:** `https://localhost:7015/api`  
- **iOS:** `https://localhost:7015/api`

### Check 4: Check Debug Output
In Visual Studio Output window, look for:
```
API Base URL: https://10.0.2.2:7015/api
Attempting login to: https://10.0.2.2:7015/api/auth/login
```

## ?? Running Both Projects Together

**Best Method:**
1. Right-click Solution in Solution Explorer
2. Click **"Configure Startup Projects"**
3. Select **"Multiple startup projects"**
4. Set both **Escale.API** and **Escale.mobile** to **"Start"**
5. Move **Escale.API** to the top (starts first)
6. Click OK
7. Now F5 starts both!

## ?? Platform-Specific Notes

### Android Emulator
- Uses `10.0.2.2` instead of `localhost`
- Already configured automatically
- Bypass SSL is enabled for development

### Windows
- Uses `localhost` directly
- Simplest to test with
- Good for initial testing

### iOS Simulator (Mac only)
- Uses `localhost`
- Requires Mac for development

## ?? Demo Credentials

The API has a hardcoded demo user for testing:
- **Username:** `admin`
- **Password:** `admin123`

This returns a mock user with one assigned station.

## ? What's Been Fixed

1. ? Created AuthController with `/api/auth/login` endpoint
2. ? Added CORS support for mobile app access
3. ? Added Swagger UI for easy API testing
4. ? Configured platform-specific URLs
5. ? Added SSL certificate bypass for development
6. ? Increased timeout to 60 seconds
7. ? Added detailed error messages
8. ? Added debug logging

## ?? Quick Troubleshooting

| Error | Solution |
|-------|----------|
| Timeout after 60 seconds | API not running - Start Escale.API |
| "Cannot connect to API" | Check firewall, verify port 7015 |
| "MethodNotAllowed" | Old error - restart mobile app |
| SSL certificate error | Should be auto-handled, restart app |

## Next: Test It!

1. ? Start API (you should see Swagger at https://localhost:7015/swagger)
2. ? Start Mobile app
3. ? Login with admin/admin123
4. ? You should see the Dashboard!

---

**Still having issues?** Check the `TROUBLESHOOTING_MOBILE_API.md` file for more detailed debugging steps.
