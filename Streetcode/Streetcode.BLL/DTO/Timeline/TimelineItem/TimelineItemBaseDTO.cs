using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.DTO.Timeline.TimelineItem
{
    public class TimelineItemBaseDto
    {
        public string Title { get; init; }
        public string Description { get; init; }
        public DateTime Date { get; init; }
        public DateViewPattern DateViewPattern { get; init; }
        public IEnumerable<HistoricalContextRequestDto>? HistoricalContexts { get; init; }
    }
}
