using FluentValidation;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Timeline.TimelineItem
{
    public class TimelineItemUpdateDtoValidator : TimelineItemBaseDtoValidator<TimelineItemUpdateDto>
    {
        public TimelineItemUpdateDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage(Errors_Validation.GreaterThan.FormatWith("Id", 0));
        }
    }
}