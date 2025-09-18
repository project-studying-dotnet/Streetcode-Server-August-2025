using FluentValidation;
using Streetcode.BLL.DTO.Partners.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Helpers;

namespace Streetcode.BLL.Validators.Partners;

public class CreatePartnerSourceLinkDTOValidator : AbstractValidator<CreatePartnerSourceLinkDTO>
{
    public const int MaxTargetUrlLength = 255;

    public CreatePartnerSourceLinkDTOValidator()
    {
        RuleFor(x => x.Id)
            .Equal(0)
            .WithMessage(Errors_Validation.Invalid.FormatWith("Id"));

        RuleFor(x => x.LogoType)
            .IsInEnum()
            .WithMessage(Errors_Validation.Invalid.FormatWith("LogoType"));

        RuleFor(x => x.TargetUrl)
            .NotEmpty()
            .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("TargetUrl"))
            .MaximumLength(MaxTargetUrlLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("TargetUrl", MaxTargetUrlLength))
            .Must(ValidationHelper.BeValidUrl)
            .WithMessage(Errors_Validation.ValidUrl.FormatWith("TargetUrl"));
    }
}