using Escale.API.DTOs.Subscriptions;
using FluentValidation;

namespace Escale.API.Validators.Subscriptions;

public class TopUpSubscriptionRequestValidator : AbstractValidator<TopUpSubscriptionRequestDto>
{
    public TopUpSubscriptionRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.TopUpAmount).GreaterThan(0).WithMessage("Top-up amount must be positive");
        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpiryDate.HasValue)
            .WithMessage("Expiry date must be in the future");
    }
}
