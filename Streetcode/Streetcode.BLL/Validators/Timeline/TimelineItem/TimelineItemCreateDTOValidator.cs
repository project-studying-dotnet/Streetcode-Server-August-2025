using FluentValidation;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Validators.Timeline.HistoricalContext;

namespace Streetcode.BLL.Validators.Timeline.TimelineItem
{
    public class TimelineItemCreateDTOValidator : TimelineItemBaseDTOValidator<TimelineItemCreateDTO>
    {
        public TimelineItemCreateDTOValidator()
        {
        }
    }
}
