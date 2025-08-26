using FluentValidation;
using Streetcode.BLL.DTO.Partners.Create;

namespace Streetcode.BLL.Validators.Partners;

public class CreatePartnerSourceLinkDTOValidator : AbstractValidator<CreatePartnerSourceLinkDTO>
{
    public CreatePartnerSourceLinkDTOValidator()
    {
        RuleFor(x => x.Id)
            .Equal(0)
            .WithMessage("Source link Id must not be set when creating.");

        RuleFor(x => x.LogoType)
            .IsInEnum()
            .WithMessage("Invalid logo type.");

        RuleFor(x => x.TargetUrl)
            .NotEmpty()
            .WithMessage("TargetUrl is required.")
            .MaximumLength(255)
            .WithMessage("TargetUrl cannot exceed 255 characters.")
            .Must(BeValidUrl)
            .WithMessage("TargetUrl must be a valid URL.");
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}