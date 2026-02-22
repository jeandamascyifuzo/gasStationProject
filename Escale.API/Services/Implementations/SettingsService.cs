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

    public SettingsService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
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

        return new EbmStatusDto
        {
            IsConnected = settings?.EBMEnabled ?? false,
            ServerUrl = settings?.EBMServerUrl ?? "",
            LastSyncAt = DateTime.UtcNow.AddMinutes(-5),
            Status = settings?.EBMEnabled == true ? "Connected" : "Disconnected"
        };
    }

    public async Task<EbmStatusDto> SyncEbmAsync()
    {
        // Mock EBM sync
        await Task.Delay(100);
        return new EbmStatusDto
        {
            IsConnected = true,
            ServerUrl = "https://ebm.rra.gov.rw",
            LastSyncAt = DateTime.UtcNow,
            Status = "Synced"
        };
    }
}
