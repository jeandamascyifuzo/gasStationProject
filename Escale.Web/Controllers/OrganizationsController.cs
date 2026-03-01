using Escale.Web.Models;
using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers;

public class OrganizationsController : Controller
{
    private readonly IApiOrganizationService _organizationService;

    public OrganizationsController(IApiOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    public async Task<IActionResult> Index()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "SuperAdmin")
            return RedirectToAction("Index", "Dashboard");

        var result = await _organizationService.GetAllAsync();

        var model = new OrganizationViewModel
        {
            Organizations = result.Data?.Select(o => new OrganizationListItem
            {
                Id = o.Id,
                Name = o.Name,
                Slug = o.Slug,
                TIN = o.TIN,
                Address = o.Address,
                Phone = o.Phone,
                Email = o.Email,
                IsActive = o.IsActive,
                CreatedAt = o.CreatedAt,
                StationCount = o.StationCount,
                UserCount = o.UserCount
            }).ToList() ?? new()
        };

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "SuperAdmin")
            return RedirectToAction("Index", "Dashboard");

        var orgTask = _organizationService.GetByIdAsync(id);
        var stationsTask = _organizationService.GetStationsAsync(id);
        var ebmTask = _organizationService.GetEbmConfigAsync(id);

        await Task.WhenAll(orgTask, stationsTask, ebmTask);

        var orgResult = orgTask.Result;
        var stationsResult = stationsTask.Result;
        var ebmResult = ebmTask.Result;

        if (!orgResult.Success || orgResult.Data == null)
        {
            TempData["ErrorMessage"] = orgResult.Message;
            return RedirectToAction("Index");
        }

        var o = orgResult.Data;
        var ebm = ebmResult.Data;
        var model = new OrganizationDetailsViewModel
        {
            Organization = new OrganizationListItem
            {
                Id = o.Id,
                Name = o.Name,
                Slug = o.Slug,
                TIN = o.TIN,
                Address = o.Address,
                Phone = o.Phone,
                Email = o.Email,
                IsActive = o.IsActive,
                CreatedAt = o.CreatedAt,
                StationCount = o.StationCount,
                UserCount = o.UserCount
            },
            Stations = stationsResult.Data?.Select(s => new Station
            {
                Id = s.Id,
                Name = s.Name,
                Location = s.Location,
                Address = s.Address,
                ContactNumber = s.PhoneNumber,
                ManagerName = s.Manager,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            }).ToList() ?? new(),
            EbmConfig = ebm != null ? new EbmConfig
            {
                EBMEnabled = ebm.EBMEnabled,
                EBMServerUrl = ebm.EBMServerUrl,
                EBMBusinessId = ebm.EBMBusinessId,
                EBMBranchId = ebm.EBMBranchId,
                EBMCompanyName = ebm.EBMCompanyName,
                EBMCompanyAddress = ebm.EBMCompanyAddress,
                EBMCompanyPhone = ebm.EBMCompanyPhone,
                EBMCompanyTIN = ebm.EBMCompanyTIN,
                EBMCategoryId = ebm.EBMCategoryId,
                IsConfigured = ebm.IsConfigured
            } : new()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name, string? tin, string? address, string? phone, string? email)
    {
        var request = new CreateOrganizationRequestDto
        {
            Name = name,
            TIN = tin,
            Address = address,
            Phone = phone,
            Email = email
        };

        var result = await _organizationService.CreateAsync(request);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Organization created successfully!" : result.Message;

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, string name, string? tin, string? address, string? phone, string? email, bool isActive)
    {
        var request = new UpdateOrganizationRequestDto
        {
            Name = name,
            TIN = tin,
            Address = address,
            Phone = phone,
            Email = email,
            IsActive = isActive
        };

        var result = await _organizationService.UpdateAsync(id, request);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Organization updated successfully!" : result.Message;

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _organizationService.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Organization deleted successfully!" : result.Message;

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ConfigureEbm(Guid orgId, bool ebmEnabled, string? ebmServerUrl,
        string? ebmBusinessId, string? ebmBranchId, string? ebmCompanyName,
        string? ebmCompanyAddress, string? ebmCompanyPhone, string? ebmCompanyTIN, string? ebmCategoryId)
    {
        var request = new EbmConfigRequestDto
        {
            EBMEnabled = ebmEnabled,
            EBMServerUrl = ebmServerUrl,
            EBMBusinessId = ebmBusinessId,
            EBMBranchId = ebmBranchId,
            EBMCompanyName = ebmCompanyName,
            EBMCompanyAddress = ebmCompanyAddress,
            EBMCompanyPhone = ebmCompanyPhone,
            EBMCompanyTIN = ebmCompanyTIN,
            EBMCategoryId = ebmCategoryId
        };

        var result = await _organizationService.ConfigureEbmAsync(orgId, request);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "EBM configuration updated!" : result.Message;

        return RedirectToAction("Details", new { id = orgId });
    }

    [HttpPost]
    public async Task<IActionResult> TestEbmConnection(Guid orgId)
    {
        var result = await _organizationService.TestEbmConnectionAsync(orgId);

        if (result.Success && result.Data)
            TempData["SuccessMessage"] = "EBM connection successful!";
        else
            TempData["ErrorMessage"] = "EBM connection failed. Check the server URL and try again.";

        return RedirectToAction("Details", new { id = orgId });
    }

    [HttpPost]
    public async Task<IActionResult> AddStation(Guid orgId, string name, string location, string? address, string? phoneNumber)
    {
        var request = new CreateStationRequestDto
        {
            Name = name,
            Location = location,
            Address = address,
            PhoneNumber = phoneNumber
        };

        var result = await _organizationService.CreateStationAsync(orgId, request);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Station added successfully!" : result.Message;

        return RedirectToAction("Details", new { id = orgId });
    }

}
