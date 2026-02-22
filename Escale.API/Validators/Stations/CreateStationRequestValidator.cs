using Escale.API.DTOs.Stations;
using FluentValidation;

namespace Escale.API.Validators.Stations;

public class CreateStationRequestValidator : AbstractValidator<CreateStationRequestDto>
{
    public CreateStationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
    }
}
