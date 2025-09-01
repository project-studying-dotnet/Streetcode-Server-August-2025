using AutoMapper;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.DAL.Entities.Streetcode;

namespace Streetcode.BLL.Mapping.Media.Images;

public class StreetcodeArtProfile : Profile
{
    public StreetcodeArtProfile()
    {
        CreateMap<StreetcodeArt, StreetcodeArtDTO>().ReverseMap();

        CreateMap<StreetcodeArtCreateUpdateDTO, StreetcodeArt>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ArtId, opt => opt.Ignore())
            .ForMember(dest => dest.Art, opt => opt.Ignore())
            .ForMember(dest => dest.StreetcodeId, opt => opt.Ignore())
            .ForMember(dest => dest.Streetcode, opt => opt.Ignore())
            .ForMember(dest => dest.StreetcodeArtSlideId, opt => opt.Ignore())
            .ForMember(dest => dest.StreetcodeArtSlide, opt => opt.Ignore())
            .ReverseMap();
    }
}
