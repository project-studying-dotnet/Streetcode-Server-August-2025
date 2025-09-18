using FluentValidation;
using Streetcode.BLL.MediatR.Team.TeamMembersLinks.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Helpers;

namespace Streetcode.BLL.Validators.Team.TeamMembersLinks;

public class CreateTeamLinkQueryValidator : AbstractValidator<CreateTeamLinkQuery>
{
    public const int MaxUrlLength = 255;

    public CreateTeamLinkQueryValidator()
    {
        RuleFor(x => x.teamMember)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequiredData.FormatWith("TeamMember"));

        When(x => x.teamMember != null, () =>
        {
            RuleFor(x => x.teamMember.LogoType)
                .IsInEnum()
                .WithMessage(Errors_Validation.Invalid.FormatWith("LogoType"));

            RuleFor(x => x.teamMember.TargetUrl)
                .NotEmpty()
                .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("TargetUrl"))
                .MaximumLength(MaxUrlLength)
                .WithMessage(Errors_Validation.MaxLength.FormatWith("TargetUrl", MaxUrlLength))
                .Must(ValidationHelper.BeValidUrl)
                .WithMessage(Errors_Validation.ValidUrl.FormatWith("TargetUrl"));

            RuleFor(x => x.teamMember.TeamMemberId)
                .GreaterThan(0)
                .WithMessage(Errors_Validation.GreaterThan.FormatWith("TeamMemberId", 0));
        });
    }
}