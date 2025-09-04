using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline.TimelineItem;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete
{
    public record DeleteTimelineItemCommand(int id) : IRequest<Result<TimelineItemDTO>>;
}
