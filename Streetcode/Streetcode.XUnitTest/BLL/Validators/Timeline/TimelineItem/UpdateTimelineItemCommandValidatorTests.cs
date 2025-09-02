using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;
using Streetcode.BLL.Validators.Timeline.TimelineItem;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Timeline.TimelineItem
{
    public class UpdateTimelineItemCommandValidatorTests
    {
        private readonly UpdateTimelineItemCommandValidator _validator;

        public UpdateTimelineItemCommandValidatorTests()
        {
            _validator = new UpdateTimelineItemCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_TimelineItem_Is_Null()
        {
            const string errorMessage = "Timeline item data is required.";

            var command = new UpdateTimelineItemCommand(null);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(c => c.TimelineItem)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_TimelineItem_Is_Invalid()
        {
            const string errorMessage = "Title cannot exceed 28 characters.";

            var invalidTimelineItem = new TimelineItemUpdateDto
            {
                Id = 1,
                Title = new string('A', 29),
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var command = new UpdateTimelineItemCommand(invalidTimelineItem);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(c => c.TimelineItem.Title)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_When_TimelineItem_Is_Valid()
        {
            var validTimelineItem = new TimelineItemUpdateDto
            {
                Id = 1,
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var command = new UpdateTimelineItemCommand(validTimelineItem);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
