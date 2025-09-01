using FluentValidation;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Validators.Helpers;

namespace Streetcode.BLL.Validators.Partners;

public class CreatePartnerDTOValidator : AbstractValidator<CreatePartnerDTO>
{
    public CreatePartnerDTOValidator()
    {
        RuleFor(x => x.Id)
            .Equal(0)
            .WithMessage("Id must not be set when creating a new partner.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(255)
            .WithMessage("Title cannot exceed 255 characters.");

        RuleFor(x => x.LogoId)
            .GreaterThan(0)
            .WithMessage("LogoId must be greater than 0.");

        RuleFor(x => x.TargetUrl)
            .MaximumLength(255)
            .WithMessage("TargetUrl cannot exceed 255 characters.")
            .Must(ValidationHelper.BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.TargetUrl))
            .WithMessage("TargetUrl must be in a valid format.");

        RuleFor(x => x.UrlTitle)
            .MaximumLength(255)
            .WithMessage("UrlTitle cannot exceed 255 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.UrlTitle));

        RuleFor(x => x.Description)
            .MaximumLength(600)
            .WithMessage("Description cannot exceed 600 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleForEach(x => x.PartnerSourceLinks)
            .SetValidator(new CreatePartnerSourceLinkDTOValidator());

        RuleForEach(x => x.Streetcodes)
            .SetValidator(new StreetcodeShortDTOValidator());
    }
}