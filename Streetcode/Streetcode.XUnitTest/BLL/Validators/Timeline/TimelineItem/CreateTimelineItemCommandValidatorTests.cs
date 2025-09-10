using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Timeline.TimelineItem;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Timeline.TimelineItem
{
    public class CreateTimelineItemCommandValidatorTests
    {
        private readonly CreateTimelineItemCommandValidator _validator;

        public CreateTimelineItemCommandValidatorTests()
        {
            _validator = new CreateTimelineItemCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_TimelineItem_Is_Null()
        {
            string errorMessage = Errors_Validation.IsRequiredData.FormatWith("TimelineItem");

            var command = new CreateTimelineItemCommand(1, null!);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(c => c.TimelineItem)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_TimelineItem_Is_Invalid()
        {
            string errorMessage = Errors_Validation.MaxLength.FormatWith("Title", 28);

            var invalidTimelineItem = new TimelineItemBaseDto
            {
                Title = new string('A', 29),
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var command = new CreateTimelineItemCommand(1, invalidTimelineItem);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(c => c.TimelineItem.Title)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_When_TimelineItem_Is_Valid()
        {
            var validTimelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var command = new CreateTimelineItemCommand(1, validTimelineItem);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
