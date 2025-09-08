using FluentValidation;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Helpers;

namespace Streetcode.BLL.Validators.Partners;

public class CreatePartnerDTOValidator : AbstractValidator<CreatePartnerDTO>
{
    public const int MaxTitleLength = 255;
    public const int MaxUrlLength = 255;
    public const int MaxUrlTitleLength = 255;
    public const int MaxDescriptionLength = 600;

    public CreatePartnerDTOValidator()
    {
        RuleFor(x => x.Id)
            .Equal(0)
            .WithMessage(Errors_Validation.Invalid.FormatWith("Id"));

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(Errors_Validation.IsRequired.FormatWith("Title"))
            .MaximumLength(MaxTitleLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("Title", MaxTitleLength));

        RuleFor(x => x.LogoId)
            .GreaterThan(0)
            .WithMessage(Errors_Validation.GreaterThan.FormatWith("LogoId", 0));

        RuleFor(x => x.TargetUrl)
            .MaximumLength(MaxUrlLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("TargetUrl", MaxUrlLength))
            .Must(ValidationHelper.BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.TargetUrl))
            .WithMessage(Errors_Validation.ValidUrl.FormatWith("TargetUrl"));

        RuleFor(x => x.UrlTitle)
            .MaximumLength(MaxUrlTitleLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("UrlTitle", MaxUrlTitleLength))
            .When(x => !string.IsNullOrWhiteSpace(x.UrlTitle));

        RuleFor(x => x.Description)
            .MaximumLength(MaxDescriptionLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("Description", MaxDescriptionLength))
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleForEach(x => x.PartnerSourceLinks)
            .SetValidator(new CreatePartnerSourceLinkDTOValidator());

        RuleForEach(x => x.Streetcodes)
            .SetValidator(new StreetcodeShortDTOValidator());
    }
}