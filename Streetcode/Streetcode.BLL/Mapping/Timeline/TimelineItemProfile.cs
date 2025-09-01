using AutoMapper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.DAL.Entities.Timeline;

namespace Streetcode.BLL.Mapping.Timeline;

public class TimelineItemProfile : Profile
{
    public TimelineItemProfile()
    {
        CreateMap<TimelineItem, TimelineItemDTO>().ReverseMap();

        CreateMap<TimelineItem, TimelineItemDTO>()
            .ForMember(dest => dest.HistoricalContexts, opt => opt.MapFrom(x => x.HistoricalContextTimelines
                .Select(x => new HistoricalContextDTO
                {
                    Id = x.HistoricalContextId,
                    Title = x.HistoricalContext.Title
                }).ToList()));

        CreateMap<TimelineItemCreateDTO, TimelineItem>()
            .ForMember(dest => dest.HistoricalContextTimelines, opt => opt.MapFrom(src => src.HistoricalContexts
                .Select(hc => new HistoricalContextTimeline
                {
                    HistoricalContext = new HistoricalContext
                    {
                        Id = hc.Id ?? 0,
                        Title = hc.Title
                    }
                }).ToList()));

        CreateMap<TimelineItemUpdateDTO, TimelineItem>()
            .ForMember(dest => dest.HistoricalContextTimelines, opt => opt.MapFrom(src => src.HistoricalContexts
                .Select(hc => new HistoricalContextTimeline
                {
                    HistoricalContext = new HistoricalContext
                    {
                        Id = hc.Id ?? 0,
                        Title = hc.Title
                    }
                }).ToList()));
    }
}
