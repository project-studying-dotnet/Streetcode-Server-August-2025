using FluentValidation;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Timeline.TimelineItem
{
    public class CreateTimelineItemCommandValidator : AbstractValidator<CreateTimelineItemCommand>
    {
        public CreateTimelineItemCommandValidator()
        {
            RuleFor(command => command.TimelineItem)
                .NotNull()
                .WithMessage(Errors_Validation.IsRequiredData.FormatWith("TimelineItem"));

            When(command => command.TimelineItem != null, () =>
            {
                RuleFor(command => command.TimelineItem)
                    .SetValidator(new TimelineItemBaseDtoValidator<TimelineItemBaseDto>());
            });
        }
    }
}
