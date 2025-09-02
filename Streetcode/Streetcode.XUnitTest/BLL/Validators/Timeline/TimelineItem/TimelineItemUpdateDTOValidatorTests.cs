using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Validators.Timeline.TimelineItem;
using Streetcode.DAL.Enums;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Timeline.TimelineItem
{
    public class TimelineItemUpdateDTOValidatorTests
    {
        private readonly TimelineItemUpdateDTOValidator _validator;

        public TimelineItemUpdateDTOValidatorTests()
        {
            _validator = new TimelineItemUpdateDTOValidator();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Should_Have_Error_When_Id_Is_Invalid(int invalidId)
        {
            const string errorMessage = "ID must be greater than 0 for an update operation.";

            var timelineItem = new TimelineItemUpdateDTO
            {
                Id = invalidId,
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDTO>()
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
            var timelineItem = new TimelineItemUpdateDTO
            {
                Id = validId,
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDTO>()
            };
            var result = _validator.TestValidate(timelineItem);
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
