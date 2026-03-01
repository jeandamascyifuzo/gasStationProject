using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.DTOs.Settings;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class SettingsService : ISettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IEBMService _ebmService;

    public SettingsService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper, IEBMService ebmService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _ebmService = ebmService;
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

        _mapper.Map(request, settings);
        _unitOfWork.OrganizationSettings.Update(settings);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<AppSettingsResponseDto>(settings);
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
}
