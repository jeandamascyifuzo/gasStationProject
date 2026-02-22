using Escale.API.DTOs.FuelTypes;
using FluentValidation;

namespace Escale.API.Validators.FuelTypes;

public class CreateFuelTypeRequestValidator : AbstractValidator<CreateFuelTypeRequestDto>
{
    public CreateFuelTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PricePerLiter).GreaterThan(0);
    }
}
