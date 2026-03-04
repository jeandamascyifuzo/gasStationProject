using Escale.Web.Helpers;
using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers;

public class ProfileController : Controller
{
    private readonly IApiAuthService _authService;

    public ProfileController(IApiAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var accessToken = HttpContext.Request.Cookies[TokenHelper.AccessTokenCookie];
        if (string.IsNullOrEmpty(accessToken))
            return RedirectToAction("Login", "Account");

        var result = await _authService.GetProfileAsync(accessToken);
        if (!result.Success || result.Data == null)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("Index", "Dashboard");
        }

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string fullName, string? email, string? phone)
    {
        var accessToken = HttpContext.Request.Cookies[TokenHelper.AccessTokenCookie];
        if (string.IsNullOrEmpty(accessToken))
            return RedirectToAction("Login", "Account");

        var request = new UpdateProfileRequestDto
        {
            FullName = fullName,
            Email = email,
            Phone = phone
        };

        var result = await _authService.UpdateProfileAsync(request, accessToken);

        if (result.Success && result.Data != null)
        {
            // Update session display name
            HttpContext.Session.SetString(TokenHelper.SessionUserFullName, result.Data.FullName);
            TempData["SuccessMessage"] = "Profile updated successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] = "New passwords do not match.";
            return RedirectToAction("Index");
        }

        var accessToken = HttpContext.Request.Cookies[TokenHelper.AccessTokenCookie];
        if (string.IsNullOrEmpty(accessToken))
            return RedirectToAction("Login", "Account");

        var request = new ChangePasswordRequestDto
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        };

        var result = await _authService.ChangePasswordAsync(request, accessToken);

        if (result.Success)
        {
            // Clear cookies and session, redirect to login
            Response.Cookies.Delete(TokenHelper.AccessTokenCookie);
            Response.Cookies.Delete(TokenHelper.RefreshTokenCookie);
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Password changed successfully. Please log in with your new password.";
            return RedirectToAction("Login", "Account");
        }

        TempData["ErrorMessage"] = result.Message;
        return RedirectToAction("Index");
    }
}
