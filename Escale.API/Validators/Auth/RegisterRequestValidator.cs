using Escale.API.DTOs.Auth;
using FluentValidation;

namespace Escale.API.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.OrganizationName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AdminUsername).NotEmpty().MaximumLength(100).Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username can only contain letters, numbers, dots, hyphens, and underscores");
        RuleFor(x => x.AdminPassword).NotEmpty().MinimumLength(6).MaximumLength(200);
        RuleFor(x => x.AdminFullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AdminEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.AdminEmail));
    }
}
