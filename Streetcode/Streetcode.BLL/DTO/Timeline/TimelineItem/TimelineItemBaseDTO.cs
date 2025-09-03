using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.DTO.Timeline.TimelineItem
{
    public class TimelineItemBaseDto
    {
        required public string Title { get; set; }
        required public string Description { get; set; }
        public DateTime Date { get; set; }
        public DateViewPattern DateViewPattern { get; set; }
        public IEnumerable<HistoricalContextRequestDto>? HistoricalContexts { get; set; }
    }
}
