using Escale.API.DTOs.Inventory;
using FluentValidation;

namespace Escale.API.Validators.Inventory;

public class CreateRefillRequestValidator : AbstractValidator<CreateRefillRequestDto>
{
    public CreateRefillRequestValidator()
    {
        RuleFor(x => x.InventoryItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThan(0);
        RuleFor(x => x.RefillDate).NotEmpty();
    }
}
