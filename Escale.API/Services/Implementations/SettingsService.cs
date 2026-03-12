using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Entities;
using Escale.API.DTOs.Settings;
using Escale.API.Hubs;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Escale.API.Services.Implementations;

public class SettingsService : ISettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IEBMService _ebmService;
    private readonly INotificationService _notificationService;
    private readonly IMemoryCache _cache;
    private readonly IAuditLogger _audit;

    public SettingsService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper,
        IEBMService ebmService, INotificationService notificationService, IMemoryCache cache, IAuditLogger audit)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _ebmService = ebmService;
        _notificationService = notificationService;
        _cache = cache;
        _audit = audit;
    }

    public async Task<AppSettingsResponseDto> GetSettingsAsync()
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var settings = await _unitOfWork.OrganizationSettings.Query()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Settings not found");
        return _mapper.Map<AppSettingsResponseDto>(settings);
    }

    public async Task<AppSettingsResponseDto> UpdateSettingsAsync(UpdateSettingsRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var settings = await _unitOfWork.OrganizationSettings.Query()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Settings not found");

        // Capture before-state for audit
        var before = _mapper.Map<AppSettingsResponseDto>(settings);

        _mapper.Map(request, settings);
        _unitOfWork.OrganizationSettings.Update(settings);
        await _unitOfWork.SaveChangesAsync();
        _cache.Remove($"org_settings_{orgId}");

        var after = _mapper.Map<AppSettingsResponseDto>(settings);
        await _audit.LogAsync("SettingsUpdate", "OrganizationSettings", orgId.ToString(), new
        {
            EBMEnabled = new { Before = before.EBMEnabled, After = after.EBMEnabled },
            EBMServerUrl = new { Before = before.EBMServerUrl, After = after.EBMServerUrl },
            CompanyName = new { Before = before.CompanyName, After = after.CompanyName }
        });

        _ = _notificationService.NotifyDataChangedAsync(orgId, NotificationConstants.SettingsChanged);
        return after;
    }

    public async Task<EbmStatusDto> GetEbmStatusAsync()
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var settings = await _unitOfWork.OrganizationSettings.Query()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId);

        var isConfigured = settings?.EBMEnabled == true
            && !string.IsNullOrEmpty(settings.EBMBusinessId)
            && !string.IsNullOrEmpty(settings.EBMBranchId);

        return new EbmStatusDto
        {
            IsConnected = isConfigured,
            ServerUrl = settings?.EBMServerUrl ?? "",
            LastSyncAt = null,
            Status = isConfigured ? "Connected" : (settings?.EBMEnabled == true ? "Incomplete Configuration" : "Disabled"),
            IsConfigured = isConfigured
        };
    }

    public async Task<EbmStatusDto> SyncEbmAsync()
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var connected = await _ebmService.TestConnectionAsync(orgId);
        return new EbmStatusDto
        {
            IsConnected = connected,
            ServerUrl = "",
            LastSyncAt = connected ? DateTime.UtcNow : null,
            Status = connected ? "Synced" : "Connection Failed"
        };
    }

    public async Task<EbmConfigResponseDto> GetEbmConfigAsync()
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var settings = await _unitOfWork.OrganizationSettings.Query()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId);

        if (settings == null)
            return new EbmConfigResponseDto();

        return new EbmConfigResponseDto
        {
            EBMEnabled = settings.EBMEnabled,
            EBMServerUrl = settings.EBMServerUrl,
            EBMBusinessId = settings.EBMBusinessId,
            EBMBranchId = settings.EBMBranchId,
            EBMCompanyName = settings.EBMCompanyName,
            EBMCompanyAddress = settings.EBMCompanyAddress,
            EBMCompanyPhone = settings.EBMCompanyPhone,
            EBMCompanyTIN = settings.EBMCompanyTIN,
            EBMCategoryId = settings.EBMCategoryId,
            IsConfigured = settings.EBMEnabled
                && !string.IsNullOrEmpty(settings.EBMBusinessId)
                && !string.IsNullOrEmpty(settings.EBMBranchId)
        };
    }

    public async Task<bool> TestEbmConnectionAsync()
    {
        var orgId = _currentUser.OrganizationId!.Value;
        return await _ebmService.TestConnectionAsync(orgId);
    }

    public async Task<List<PaymentMethodSettingDto>> GetPaymentMethodsAsync()
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var methods = await _unitOfWork.PaymentMethods.Query()
            .Where(p => p.OrganizationId == orgId)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        // Auto-create defaults if this org has never had payment methods seeded
        if (methods.Count == 0)
        {
            var now = DateTime.UtcNow;
            var defaults = new[]
            {
                ("Cash",        "Cash",         1),
                ("MobileMoney", "Mobile Money",  2),
                ("Card",        "Card",          3),
                ("Credit",      "Credit",        4),
            };
            foreach (var (name, displayName, sortOrder) in defaults)
            {
                var m = new OrganizationPaymentMethod
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgId,
                    Name = name,
                    DisplayName = displayName,
                    IsEnabled = true,
                    SortOrder = sortOrder,
                    CreatedAt = now
                };
                await _unitOfWork.PaymentMethods.AddAsync(m);
                methods.Add(m);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        return methods.Select(p => new PaymentMethodSettingDto
        {
            Id = p.Id,
            Name = p.Name,
            DisplayName = p.DisplayName,
            IsEnabled = p.IsEnabled,
            SortOrder = p.SortOrder
        }).ToList();
    }

    public async Task<PaymentMethodSettingDto> UpdatePaymentMethodAsync(Guid id, UpdatePaymentMethodDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var method = await _unitOfWork.PaymentMethods.Query()
            .FirstOrDefaultAsync(p => p.Id == id && p.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Payment method not found");

        method.IsEnabled = request.IsEnabled;
        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            method.DisplayName = request.DisplayName.Trim();

        _unitOfWork.PaymentMethods.Update(method);
        await _unitOfWork.SaveChangesAsync();

        await _audit.LogAsync("PaymentMethodUpdate", "OrganizationPaymentMethod", id.ToString(), new
        {
            Name = method.Name, IsEnabled = request.IsEnabled, DisplayName = method.DisplayName
        });

        return new PaymentMethodSettingDto
        {
            Id = method.Id,
            Name = method.Name,
            DisplayName = method.DisplayName,
            IsEnabled = method.IsEnabled,
            SortOrder = method.SortOrder
        };
    }
}
