using Escale.API.DTOs.Settings;
using FluentValidation;

namespace Escale.API.Validators.Settings;

public class EbmConfigRequestValidator : AbstractValidator<EbmConfigRequestDto>
{
    public EbmConfigRequestValidator()
    {
        When(x => x.EBMEnabled, () =>
        {
            RuleFor(x => x.EBMServerUrl)
                .NotEmpty().WithMessage("EBM Server URL is required when EBM is enabled")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("EBM Server URL must be a valid URL");

            RuleFor(x => x.EBMBusinessId)
                .NotEmpty().WithMessage("EBM Business ID is required when EBM is enabled")
                .Must(id => Guid.TryParse(id, out _))
                .WithMessage("EBM Business ID must be a valid GUID");

            RuleFor(x => x.EBMBranchId)
                .NotEmpty().WithMessage("EBM Branch ID is required when EBM is enabled")
                .Must(id => Guid.TryParse(id, out _))
                .WithMessage("EBM Branch ID must be a valid GUID");

            RuleFor(x => x.EBMCompanyName)
                .NotEmpty().WithMessage("EBM Company Name is required when EBM is enabled");

            RuleFor(x => x.EBMCompanyTIN)
                .NotEmpty().WithMessage("EBM Company TIN is required when EBM is enabled");
        });
    }
}
