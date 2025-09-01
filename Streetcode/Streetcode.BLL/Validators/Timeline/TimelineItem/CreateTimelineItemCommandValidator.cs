using FluentValidation;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;

namespace Streetcode.BLL.Validators.Timeline.TimelineItem
{
    public class CreateTimelineItemCommandValidator : AbstractValidator<CreateTimelineItemCommand>
    {
        public CreateTimelineItemCommandValidator()
        {
            RuleFor(command => command.TimelineItem)
                .NotNull()
                .WithMessage("Timeline item data is required.");

            When(command => command.TimelineItem != null, () =>
            {
                RuleFor(command => command.TimelineItem)
                    .SetValidator(new TimelineItemCreateDTOValidator());
            });
        }
    }
}
