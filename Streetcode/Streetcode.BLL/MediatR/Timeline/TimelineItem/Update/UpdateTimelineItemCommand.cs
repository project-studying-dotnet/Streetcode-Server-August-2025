using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline.TimelineItem;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Update
{
    public record UpdateTimelineItemCommand(TimelineItemUpdateDTO TimelineItem) : IRequest<Result<TimelineItemDTO>>;
}
