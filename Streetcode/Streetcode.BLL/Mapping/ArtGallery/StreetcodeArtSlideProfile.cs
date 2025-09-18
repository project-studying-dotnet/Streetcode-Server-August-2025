using AutoMapper;
using Streetcode.BLL.DTO.ArtGallery;
using Streetcode.DAL.Entities.Streetcode;

namespace Streetcode.BLL.Mapping.ArtGallery;

public class StreetcodeArtSlideProfile : Profile
{
    public StreetcodeArtSlideProfile()
    {
        CreateMap<StreetcodeArtSlideDTO, StreetcodeArtSlide>().ReverseMap();

        CreateMap<StreetcodeArtSlideCreateUpdateDTO, StreetcodeArtSlide>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.StreetcodeId, opt => opt.Ignore())
            .ForMember(dest => dest.Streetcode, opt => opt.Ignore())
            .ForMember(dest => dest.StreetcodeArts, opt => opt.Ignore())
            .ReverseMap();
    }
}
