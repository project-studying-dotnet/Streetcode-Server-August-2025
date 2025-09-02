using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
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
            const string errorMessage = "Timeline item data is required.";

            var command = new CreateTimelineItemCommand(null);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(c => c.TimelineItem)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_TimelineItem_Is_Invalid()
        {
            const string errorMessage = "Title cannot exceed 28 characters.";

            var invalidTimelineItem = new TimelineItemCreateDto
            {
                Title = new string('A', 29),
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var command = new CreateTimelineItemCommand(invalidTimelineItem);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(c => c.TimelineItem.Title)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_When_TimelineItem_Is_Valid()
        {
            var validTimelineItem = new TimelineItemCreateDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var command = new CreateTimelineItemCommand(validTimelineItem);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
