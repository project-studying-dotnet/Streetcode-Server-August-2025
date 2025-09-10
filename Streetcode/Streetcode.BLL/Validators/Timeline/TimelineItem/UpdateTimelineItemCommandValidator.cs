using FluentValidation;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Timeline.TimelineItem
{
    public class UpdateTimelineItemCommandValidator : AbstractValidator<UpdateTimelineItemCommand>
    {
        public UpdateTimelineItemCommandValidator()
        {
            RuleFor(command => command.TimelineItem)
                .NotNull()
                .WithMessage(Errors_Validation.IsRequiredData.FormatWith("TimelineItem"));

            When(command => command.TimelineItem != null, () =>
            {
                RuleFor(command => command.TimelineItem)
                    .SetValidator(new TimelineItemUpdateDtoValidator());
            });
        }
    }
}
