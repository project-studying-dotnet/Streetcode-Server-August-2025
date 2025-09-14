using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.MediatR.Team.TeamMembersLinks.Create;
using Streetcode.BLL.Validators.Team.TeamMembersLinks;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Team.TeamMembersLinks
{
    public class CreateTeamLinkQueryValidatorTests
    {
        private readonly CreateTeamLinkQueryValidator _validator;

        public CreateTeamLinkQueryValidatorTests()
        {
            _validator = new CreateTeamLinkQueryValidator();
        }

        [Fact]
        public void Should_Have_Error_When_TeamMember_Is_Null()
        {
            var command = new CreateTeamLinkQuery(null);
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.teamMember);
        }

        [Fact]
        public void Should_Not_Have_Error_When_TeamMember_Valid()
        {
            var command = new CreateTeamLinkQuery(new TeamMemberLinkDTO
            {
                LogoType = LogoTypeDTO.Twitter,
                TargetUrl = "https://example.com",
                TeamMemberId = 1
            });

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.teamMember.LogoType);
            result.ShouldNotHaveValidationErrorFor(x => x.teamMember.TargetUrl);
            result.ShouldNotHaveValidationErrorFor(x => x.teamMember.TeamMemberId);
        }
    }
}
