using Escale.API.DTOs.Users;
using FluentValidation;

namespace Escale.API.Validators.Users;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).NotEmpty().Must(r => r is "Admin" or "Manager" or "Cashier");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}
