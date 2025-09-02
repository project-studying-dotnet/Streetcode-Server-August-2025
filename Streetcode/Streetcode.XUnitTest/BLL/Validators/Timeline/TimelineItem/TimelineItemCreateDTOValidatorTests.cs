using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Validators.Timeline.TimelineItem;
using Streetcode.DAL.Enums;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Timeline.TimelineItem
{
    public class TimelineItemCreateDTOValidatorTests
    {
        private readonly TimelineItemCreateDTOValidator _validator;

        public TimelineItemCreateDTOValidatorTests()
        {
            _validator = new TimelineItemCreateDTOValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Title_Exceeds_Max_Length()
        {
            const string errorMessage = "Title cannot exceed 28 characters.";

            var timelineItem = new TimelineItemCreateDTO
            {
                Title = new string('A', 29),
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDTO>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_Max_Length()
        {
            const string errorMessage = "Description cannot exceed 400 characters.";

            var timelineItem = new TimelineItemCreateDTO
            {
                Title = "Valid Title",
                Description = new string('A', 401),
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDTO>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Date_Is_In_The_Future()
        {
            const string errorMessage = "Date cannot be from the future.";

            var timelineItem = new TimelineItemCreateDTO
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now.AddDays(1),
                HistoricalContexts = new List<HistoricalContextRequestDTO>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Date)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
        {
            var timelineItem = new TimelineItemCreateDTO
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDTO>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            const string errorMessage = "Title is required.";

            var timelineItem = new TimelineItemCreateDTO
            {
                Title = "",
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDTO>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Description_Is_Empty()
        {
            const string errorMessage = "Description is required.";

            var timelineItem = new TimelineItemCreateDTO
            {
                Title = "Valid Title",
                Description = "",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDTO>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Date_Is_Empty()
        {
            const string errorMessage = "Date is required.";

            var timelineItem = new TimelineItemCreateDTO
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = default,
                HistoricalContexts = new List<HistoricalContextRequestDTO>()
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor(x => x.Date)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_When_HistoricalContexts_Are_Valid()
        {
            var timelineItem = new TimelineItemCreateDTO
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDTO>
                {
                    new HistoricalContextRequestDTO { Id = 1, Title = null },
                    new HistoricalContextRequestDTO { Id = null, Title = "Valid Context" }
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

            var timelineItem = new TimelineItemCreateDTO
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDTO>
                {
                    new HistoricalContextRequestDTO { Id = null, Title = null },
                    new HistoricalContextRequestDTO { Id = 1, Title = "Valid Context" }
                }
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldHaveValidationErrorFor("HistoricalContexts[0]")
                  .WithErrorMessage(errorMessage1);

            result.ShouldHaveValidationErrorFor("HistoricalContexts[1]")
                  .WithErrorMessage(errorMessage2);
        }

        [Fact]
        public void Should_Not_Have_Error_When_HistoricalContexts_Are_Null()
        {
            var timelineItem = new TimelineItemCreateDTO
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = null,
            };

            var result = _validator.TestValidate(timelineItem);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Have_Error_When_DateViewPattern_Is_Invalid()
        {
            const string errorMessage = "Provided date view pattern is not a valid value.";

            var timelineItem = new TimelineItemCreateDTO
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                DateViewPattern = (DateViewPattern)999,
                HistoricalContexts = new List<HistoricalContextRequestDTO>()
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
            var timelineItem = new TimelineItemCreateDTO
            {
                Title = new string('A', validLength),
                Description = "Valid Description",
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDTO>()
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
            var timelineItem = new TimelineItemCreateDTO
            {
                Title = "Valid Title",
                Description = new string('A', validLength),
                Date = DateTime.Now.AddYears(-1),
                HistoricalContexts = new List<HistoricalContextRequestDTO>()
            };
            var result = _validator.TestValidate(timelineItem);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }
    }
}
