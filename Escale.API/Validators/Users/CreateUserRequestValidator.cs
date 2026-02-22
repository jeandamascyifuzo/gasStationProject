using Escale.API.DTOs.Users;
using FluentValidation;

namespace Escale.API.Validators.Users;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequestDto>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100).Matches("^[a-zA-Z0-9._-]+$");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(200);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).NotEmpty().Must(r => r is "Admin" or "Manager" or "Cashier").WithMessage("Role must be Admin, Manager, or Cashier");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}
