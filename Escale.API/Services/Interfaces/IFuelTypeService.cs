using Escale.API.DTOs.FuelTypes;

namespace Escale.API.Services.Interfaces;

public interface IFuelTypeService
{
    Task<List<FuelTypeResponseDto>> GetFuelTypesAsync();
    Task<FuelTypeResponseDto> GetFuelTypeByIdAsync(Guid id);
    Task<FuelTypeResponseDto> CreateFuelTypeAsync(CreateFuelTypeRequestDto request);
    Task<FuelTypeResponseDto> UpdateFuelTypeAsync(Guid id, UpdateFuelTypeRequestDto request);
    Task DeleteFuelTypeAsync(Guid id);
}
