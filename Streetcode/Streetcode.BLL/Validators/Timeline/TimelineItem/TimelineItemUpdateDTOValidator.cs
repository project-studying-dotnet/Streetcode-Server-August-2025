using FluentValidation;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Validators.Timeline.HistoricalContext;

public class TimelineItemUpdateDTOValidator : AbstractValidator<TimelineItemUpdateDTO>
{
    public TimelineItemUpdateDTOValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID must be greater than 0 for an update operation.");

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