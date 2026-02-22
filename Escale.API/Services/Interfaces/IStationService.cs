using Escale.API.DTOs.Common;
using Escale.API.DTOs.Stations;

namespace Escale.API.Services.Interfaces;

public interface IStationService
{
    Task<List<StationResponseDto>> GetStationsAsync();
    Task<StationDetailResponseDto> GetStationByIdAsync(Guid id);
    Task<StationResponseDto> CreateStationAsync(CreateStationRequestDto request);
    Task<StationResponseDto> UpdateStationAsync(Guid id, UpdateStationRequestDto request);
    Task DeleteStationAsync(Guid id);
}
