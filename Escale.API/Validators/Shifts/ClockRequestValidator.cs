using Escale.API.DTOs.Shifts;
using FluentValidation;

namespace Escale.API.Validators.Shifts;

public class ClockRequestValidator : AbstractValidator<ClockRequestDto>
{
    public ClockRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.StationId).NotEmpty();
    }
}
