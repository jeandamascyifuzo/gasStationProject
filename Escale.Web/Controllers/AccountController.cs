using Escale.Web.Helpers;
using Escale.Web.Models;
using Escale.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiAuthService _authService;

        public AccountController(IApiAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Login()
        {
            // If already logged in, redirect appropriately
            var token = HttpContext.Request.Cookies[TokenHelper.AccessTokenCookie];
            if (!string.IsNullOrEmpty(token))
            {
                if (HttpContext.Session.GetString(TokenHelper.SessionUserRole) == "SuperAdmin")
                    return RedirectToAction("Index", "Organizations");
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model.Username, model.Password, model.RememberMe);

            if (!result.Success)
            {
                model.ErrorMessage = result.Message;
                return View(model);
            }

            // Block Cashier from admin portal — they use the mobile app
            if (result.User?.Role == "Cashier")
            {
                model.ErrorMessage = "You don't have permission to access the admin portal. Please use the mobile app or contact your administrator.";
                return View(model);
            }

            // Store tokens in HttpOnly secure cookies
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(1)
            };

            Response.Cookies.Append(TokenHelper.AccessTokenCookie, result.Token, cookieOptions);
            Response.Cookies.Append(TokenHelper.RefreshTokenCookie, result.RefreshToken, cookieOptions);

            // Store user info in session for display purposes
            if (result.User != null)
            {
                HttpContext.Session.SetString(TokenHelper.SessionUserFullName, result.User.FullName);
                HttpContext.Session.SetString(TokenHelper.SessionUserRole, result.User.Role);
                HttpContext.Session.SetString(TokenHelper.SessionUserId, result.User.Id.ToString());
                if (!string.IsNullOrEmpty(result.User.OrganizationName))
                    HttpContext.Session.SetString(TokenHelper.SessionOrgName, result.User.OrganizationName);
                if (!string.IsNullOrEmpty(result.User.LogoUrl))
                    HttpContext.Session.SetString(TokenHelper.SessionLogoUrl, result.User.LogoUrl);
                if (result.User.AssignedStations.Any())
                {
                    var stationIds = string.Join(",", result.User.AssignedStations.Select(s => s.Id));
                    HttpContext.Session.SetString(TokenHelper.SessionStationIds, stationIds);
                }
            }

            // Redirect to splash screen which sets up the app then navigates
            return RedirectToAction("Splash");
        }

        public IActionResult Splash()
        {
            var token = HttpContext.Request.Cookies[TokenHelper.AccessTokenCookie];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var role = HttpContext.Session.GetString(TokenHelper.SessionUserRole);
            if (string.IsNullOrEmpty(role))
            {
                // No session data — clear everything and go to login
                Response.Cookies.Delete(TokenHelper.AccessTokenCookie);
                Response.Cookies.Delete(TokenHelper.RefreshTokenCookie);
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            ViewBag.Role = role;
            ViewBag.OrgName = HttpContext.Session.GetString(TokenHelper.SessionOrgName) ?? "";
            ViewBag.LogoUrl = HttpContext.Session.GetString(TokenHelper.SessionLogoUrl) ?? "";
            ViewBag.UserName = HttpContext.Session.GetString(TokenHelper.SessionUserFullName) ?? "User";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            var accessToken = HttpContext.Request.Cookies[TokenHelper.AccessTokenCookie];
            var refreshToken = HttpContext.Request.Cookies[TokenHelper.RefreshTokenCookie];

            // Revoke token on server
            if (!string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(accessToken))
            {
                await _authService.RevokeTokenAsync(refreshToken, accessToken);
            }

            // Clear cookies
            Response.Cookies.Delete(TokenHelper.AccessTokenCookie);
            Response.Cookies.Delete(TokenHelper.RefreshTokenCookie);

            // Clear session
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
