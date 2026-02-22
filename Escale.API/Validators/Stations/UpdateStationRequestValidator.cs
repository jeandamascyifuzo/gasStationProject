using Escale.API.DTOs.Stations;
using FluentValidation;

namespace Escale.API.Validators.Stations;

public class UpdateStationRequestValidator : AbstractValidator<UpdateStationRequestDto>
{
    public UpdateStationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
    }
}
