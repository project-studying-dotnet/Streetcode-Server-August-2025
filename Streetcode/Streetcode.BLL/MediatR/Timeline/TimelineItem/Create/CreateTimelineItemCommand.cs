using MediatR;
using FluentResults;
using Streetcode.BLL.DTO.Timeline.TimelineItem;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Create
{
    public record CreateTimelineItemCommand(int streetcodeId, TimelineItemBaseDto TimelineItem) : IRequest<Result<TimelineItemDTO>>;
}
