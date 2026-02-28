using Escale.API.DTOs.Subscriptions;
using FluentValidation;

namespace Escale.API.Validators.Subscriptions;

public class LookupCarRequestValidator : AbstractValidator<LookupCarRequestDto>
{
    public LookupCarRequestValidator()
    {
        RuleFor(x => x.PlateNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PIN).NotEmpty().Length(4).Matches("^[0-9]{4}$")
            .WithMessage("PIN must be exactly 4 digits");
        RuleFor(x => x.SaleAmount).GreaterThan(0).When(x => x.SaleAmount.HasValue);
    }
}
