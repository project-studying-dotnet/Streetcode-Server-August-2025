using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Timeline.TimelineItem
{
    public class TimelineItemUpdateDtoValidatorTests
    {
        private readonly TimelineItemUpdateDtoValidator _validator;

        public TimelineItemUpdateDtoValidatorTests()
        {
            _validator = new TimelineItemUpdateDtoValidator();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Should_Have_Error_When_Id_Is_Invalid(int invalidId)
        {
            const string errorMessage = "ID must be greater than 0 for an update operation.";

            var timelineItem = new TimelineItemUpdateDto
            {
                Id = invalidId,
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.UtcNow.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage(errorMessage);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(500)]
        [InlineData(999)]
        public void Should_Not_Have_Error_When_Id_Is_Valid(int validId)
        {
            var timelineItem = new TimelineItemUpdateDto
            {
                Id = validId,
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.UtcNow,
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };
            var result = _validator.TestValidate(timelineItem);
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
