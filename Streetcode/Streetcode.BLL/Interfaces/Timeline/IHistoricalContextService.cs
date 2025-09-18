using FluentResults;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.DAL.Entities.Timeline;

namespace Streetcode.BLL.Interfaces.Timeline
{
    public interface IHistoricalContextService
    {
        Task<Result> CheckForDuplicateTitlesAsync(IEnumerable<HistoricalContextRequestDto> contexts);
        Task<Result> BuildHistoricalContextLinksAsync(TimelineItem timelineItem, IEnumerable<HistoricalContextRequestDto> contexts);
        Result RemoveObsoleteLinks(TimelineItem timelineItem, IEnumerable<HistoricalContextRequestDto> newContexts);
    }
}