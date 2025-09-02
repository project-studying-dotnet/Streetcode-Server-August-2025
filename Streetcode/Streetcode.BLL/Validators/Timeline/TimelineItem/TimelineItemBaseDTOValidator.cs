using FluentValidation;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Validators.Timeline.HistoricalContext;

namespace Streetcode.BLL.Validators.Timeline.TimelineItem
{
    public class TimelineItemBaseDTOValidator<T> : AbstractValidator<T>
        where T : TimelineItemBaseDTO
    {
        public TimelineItemBaseDTOValidator()
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
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("Date cannot be from the future.");

            RuleFor(x => x.DateViewPattern)
                .IsInEnum()
                .WithMessage("Provided date view pattern is not a valid value.");

            RuleForEach(x => x.HistoricalContexts)
                .SetValidator(new HistoricalContextRequestDTOValidator())
                .When(x => x.HistoricalContexts != null);
        }
    }
}
