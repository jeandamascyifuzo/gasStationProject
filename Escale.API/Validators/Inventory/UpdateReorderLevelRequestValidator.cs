using Escale.API.DTOs.Inventory;
using FluentValidation;

namespace Escale.API.Validators.Inventory;

public class UpdateReorderLevelRequestValidator : AbstractValidator<UpdateReorderLevelRequestDto>
{
    public UpdateReorderLevelRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
    }
}
