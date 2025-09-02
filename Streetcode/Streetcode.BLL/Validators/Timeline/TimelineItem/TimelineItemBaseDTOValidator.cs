using FluentValidation;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Validators.Timeline.HistoricalContext;

namespace Streetcode.BLL.Validators.Timeline.TimelineItem
{
    public class TimelineItemBaseDtoValidator<T> : AbstractValidator<T>
        where T : TimelineItemBaseDto
    {
        public TimelineItemBaseDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.")
                .MaximumLength(28)
                .WithMessage("Title cannot exceed 28 characters.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required.")
                .MaximumLength(400)
                .WithMessage("Description cannot exceed 400 characters.");

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("Date is required.")
                .LessThanOrEqualTo(_ => DateTime.UtcNow)
                .WithMessage("Date cannot be in the future.");

            RuleFor(x => x.DateViewPattern)
                .IsInEnum()
                .WithMessage("Provided date view pattern is not a valid value.");

            RuleForEach(x => x.HistoricalContexts)
                .SetValidator(new HistoricalContextRequestDtoValidator())
                .When(x => x.HistoricalContexts != null);
        }
    }
}
