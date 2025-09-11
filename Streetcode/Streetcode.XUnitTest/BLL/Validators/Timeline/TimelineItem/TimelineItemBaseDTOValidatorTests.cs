using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
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
            int maxTitleLength = TimelineItemBaseDtoValidator<TimelineItemBaseDto>.TitleMaxLength;
            string errorMessage = Errors_Validation.MaxLength.FormatWith("Title", maxTitleLength);

            var timelineItem = new TimelineItemBaseDto
            {
                Title = new string('A', maxTitleLength + 1),
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
            int maxDescriptionLength = TimelineItemBaseDtoValidator<TimelineItemBaseDto>.DescriptionMaxLength;
            string errorMessage = Errors_Validation.MaxLength.FormatWith("Description", maxDescriptionLength);

            var timelineItem = new TimelineItemBaseDto
            {
                Title = "Valid Title",
                Description = new string('A', maxDescriptionLength + 1),
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
            string errorMessage = Errors_Validation.MustBeInPast.FormatWith("Date");

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
            string errorMessage = Errors_Validation.CannotBeEmpty.FormatWith("Title");

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
            string errorMessage = Errors_Validation.CannotBeEmpty.FormatWith("Description");

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
            string errorMessage = Errors_Validation.CannotBeEmpty.FormatWith("Date");

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
            string errorMessage = Errors_Validation.IsRequired.FormatWith("HistoricalContext");

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
            string errorMessage = Errors_Validation.Invalid.FormatWith("DateViewPattern");

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
