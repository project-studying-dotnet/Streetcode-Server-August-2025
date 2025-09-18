using FluentValidation;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Timeline.HistoricalContext;

namespace Streetcode.BLL.Validators.Timeline.TimelineItem
{
    public class TimelineItemBaseDtoValidator<T> : AbstractValidator<T>
        where T : TimelineItemBaseDto
    {
        public const int TitleMaxLength = 28;
        public const int DescriptionMaxLength = 400;

        public TimelineItemBaseDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Title"))
                .MaximumLength(TitleMaxLength)
                .WithMessage(Errors_Validation.MaxLength.FormatWith("Title", TitleMaxLength));

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Description"))
                .MaximumLength(DescriptionMaxLength)
                .WithMessage(Errors_Validation.MaxLength.FormatWith("Description", DescriptionMaxLength));

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Date"))
                .LessThanOrEqualTo(_ => DateTime.UtcNow)
                .WithMessage(Errors_Validation.MustBeInPast.FormatWith("Date"));

            RuleFor(x => x.DateViewPattern)
                .IsInEnum()
                .WithMessage(Errors_Validation.Invalid.FormatWith("DateViewPattern"));

            RuleForEach(x => x.HistoricalContexts)
                .NotNull()
                .WithMessage(Errors_Validation.IsRequired.FormatWith("HistoricalContext"))
                .SetValidator(new HistoricalContextRequestDtoValidator());
        }
    }
}
