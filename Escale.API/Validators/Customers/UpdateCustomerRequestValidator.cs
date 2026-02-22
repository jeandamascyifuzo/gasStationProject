using Escale.API.DTOs.Customers;
using FluentValidation;

namespace Escale.API.Validators.Customers;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequestDto>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
    }
}
