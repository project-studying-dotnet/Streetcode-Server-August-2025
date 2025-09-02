using FluentValidation;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;

namespace Streetcode.BLL.Validators.Timeline.TimelineItem
{
    public class UpdateTimelineItemCommandValidator : AbstractValidator<UpdateTimelineItemCommand>
    {
        public UpdateTimelineItemCommandValidator()
        {
            RuleFor(command => command.TimelineItem)
                .NotNull()
                .WithMessage("Timeline item data is required.");

            When(command => command.TimelineItem != null, () =>
            {
                RuleFor(command => command.TimelineItem)
                    .SetValidator(new TimelineItemUpdateDtoValidator());
            });
        }
    }
}
