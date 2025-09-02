using AutoMapper;
using Streetcode.BLL.DTO.Locations;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.DTO.Locations.Update;
using Streetcode.DAL.Entities.Analytics;

namespace Streetcode.BLL.Mapping.Locations;

public class MapPointProfile : Profile
{
    public MapPointProfile()
    {
        CreateMap<StatisticRecord, MapPointDTO>()
            .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.Count))
            .ForMember(dest => dest.StreetcodeCoordinate, opt => opt.MapFrom(src => src.StreetcodeCoordinate))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ReverseMap();

        CreateMap<StatisticRecord, MapPointResponseDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.Count))
            .ForMember(dest => dest.StreetcodeCoordinate, opt => opt.MapFrom(src => src.StreetcodeCoordinate))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ReverseMap();

        CreateMap<MapPointUpdateDTO, StreetcodeCoordinate>()
            .ForMember(sc => sc.Id, conf => conf.MapFrom(sru => sru.StreetcodeCoordinate.Id))
            .ForMember(sc => sc.Latitude, conf => conf.MapFrom(sru => sru.StreetcodeCoordinate.Latitude))
            .ForMember(sc => sc.Longtitude, conf => conf.MapFrom(sru => sru.StreetcodeCoordinate.Longtitude))
            .ForMember(sc => sc.StatisticRecord, conf => conf.MapFrom(sru =>
            new StatisticRecord()
            {
                Id = sru.Id,
                Count = sru.PlateNumber,
                Address = sru.Address,
                StreetcodeId = sru.StreetcodeId,
                StreetcodeCoordinateId = sru.StreetcodeCoordinate.Id,
            }));
    }
}
