using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.DTO.Timeline.TimelineItem
{
    public class TimelineItemCreateDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public DateViewPattern DateViewPattern { get; set; }
        public IEnumerable<HistoricalContextRequestDTO> HistoricalContexts { get; set; }
    }
}
