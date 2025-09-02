using FluentValidation;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Validators.Timeline.HistoricalContext;
using Streetcode.BLL.Validators.Timeline.TimelineItem;

public class TimelineItemUpdateDTOValidator : TimelineItemBaseDTOValidator<TimelineItemUpdateDTO>
{
    public TimelineItemUpdateDTOValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID must be greater than 0 for an update operation.");
    }
}