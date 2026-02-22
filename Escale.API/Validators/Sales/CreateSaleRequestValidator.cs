using Escale.API.DTOs.Sales;
using FluentValidation;

namespace Escale.API.Validators.Sales;

public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequestDto>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.StationId).NotEmpty();
        RuleFor(x => x.Liters).GreaterThan(0);
        RuleFor(x => x.PricePerLiter).GreaterThan(0);
        RuleFor(x => x.PaymentMethod).NotEmpty();
    }
}
