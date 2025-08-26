using FluentValidation;
using Streetcode.BLL.MediatR.Team.TeamMembersLinks.Create;
using Streetcode.BLL.Validators.Helpers;

namespace Streetcode.BLL.Validators.Team.TeamMembersLinks;

public class CreateTeamLinkQueryValidator : AbstractValidator<CreateTeamLinkQuery>
{
    public CreateTeamLinkQueryValidator()
    {
        RuleFor(x => x.teamMember)
            .NotNull()
            .WithMessage("Team member link data is required.");

        When(x => x.teamMember != null, () =>
        {
            RuleFor(x => x.teamMember.LogoType)
                .IsInEnum()
                .WithMessage("LogoType must be a valid value.");

            RuleFor(x => x.teamMember.TargetUrl)
                .NotEmpty()
                .WithMessage("Target URL is required.")
                .MaximumLength(255)
                .WithMessage("Target URL cannot exceed 255 characters.")
                .Must(ValidationHelper.BeValidUrl)
                .WithMessage("Target URL must be a valid absolute URL.");

            RuleFor(x => x.teamMember.TeamMemberId)
                .GreaterThan(0)
                .WithMessage("TeamMemberId must be greater than 0.");
        });
    }

}