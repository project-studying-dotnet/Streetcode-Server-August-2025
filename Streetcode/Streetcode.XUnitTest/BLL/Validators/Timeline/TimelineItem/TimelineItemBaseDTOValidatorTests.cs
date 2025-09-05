using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Validators.Timeline.TimelineItem;
using Streetcode.DAL.Enums;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Timeline.TimelineItem
{
    public class TimelineItemBaseDtoValidatorTests
    {
        private readonly TimelineItemBaseDtoValidator<TimelineItemBaseDto> _validator;

        public TimelineItemBaseDtoValidatorTests()
        {
            _validator = new TimelineItemBaseDtoValidator<TimelineItemBaseDto>();
        }

        [Fact]
        public void Should_Have_Error_When_Title_Exceeds_Max_Length()
        {
            const string errorMessage = "Title cannot exceed 28 characters.";

            var timelineItem = new TimelineItemBaseDto
            {
                Title = new string('A', 29),
                Description = "Valid Description",
                Date = DateTime.UtcNow,
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_Max_Length()
        {
            const string errorMessage = "Description cannot exceed 400 characters.";

            var timelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = new string('A', 401),
                Date = DateTime.UtcNow,
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Date_Is_In_The_Future()
        {
            const string errorMessage = "Date cannot be in the future.";

            var timelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.UtcNow.AddDays(1),
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Date)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
        {
            var timelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.UtcNow,
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            const string errorMessage = "Title is required.";

            var timelineItem = new TimelineItemBaseDto
            {
                Title = "  ",
                Description = "Valid Description",
                Date = DateTime.UtcNow,
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Description_Is_Empty()
        {
            const string errorMessage = "Description is required.";

            var timelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = "",
                Date = DateTime.UtcNow,
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Date_Is_Empty()
        {
            const string errorMessage = "Date is required.";

            var timelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = default,
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Date)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_When_HistoricalContexts_Are_Valid()
        {
            var timelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.UtcNow,
                HistoricalContexts = new List<HistoricalContextRequestDto>
                {
                    new HistoricalContextRequestDto { Id = 1, Title = null },
                    new HistoricalContextRequestDto { Id = null, Title = "Valid Context" }
                }
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Have_Error_When_HistoricalContext_Is_Invalid()
        {
            const string errorMessage1 = "Context must have either an ID or a title.";
            const string errorMessage2 = "Cannot provide both an ID and a title for one context.";

            var timelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.UtcNow,
                HistoricalContexts = new List<HistoricalContextRequestDto>
                {
                    new HistoricalContextRequestDto { Id = null, Title = null },
                    new HistoricalContextRequestDto { Id = 1, Title = "Valid Context" }
                }
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor("HistoricalContexts[0]")
                  .WithErrorMessage(errorMessage1);

            result.ShouldHaveValidationErrorFor("HistoricalContexts[1]")
                  .WithErrorMessage(errorMessage2);
        }

        [Fact]
        public void Should_Have_Error_When_HistoricalContexts_Contain_Null_Element()
        {
            const string errorMessage = "Historical context cannot be null.";

            var timelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.UtcNow,
                HistoricalContexts = new List<HistoricalContextRequestDto>
                {
                    new HistoricalContextRequestDto { Id = 1, Title = null },
                    null!
                }
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor("HistoricalContexts[1]")
                    .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_When_HistoricalContexts_Are_Null()
        {
            var timelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.UtcNow,
                HistoricalContexts = null,
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Have_Error_When_DateViewPattern_Is_Invalid()
        {
            const string errorMessage = "Provided date view pattern is not a valid value.";

            var timelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.UtcNow,
                DateViewPattern = (DateViewPattern)999,
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };
            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.DateViewPattern)
                  .WithErrorMessage(errorMessage);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(14)]
        [InlineData(27)]
        [InlineData(28)]
        public void Should_Not_Have_Error_When_Title_Length_Is_Valid(int validLength)
        {
            var timelineItem = new TimelineItemBaseDto
            {
                Title = new string('A', validLength),
                Description = "Valid Description",
                Date = DateTime.UtcNow,
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(200)]
        [InlineData(399)]
        [InlineData(400)]
        public void Should_Not_Have_Error_When_Description_Length_Is_Valid(int validLength)
        {
            var timelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = new string('A', validLength),
                Date = DateTime.UtcNow,
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };
            var result = _validator.TestValidate(timelineItem);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }
    }
}
