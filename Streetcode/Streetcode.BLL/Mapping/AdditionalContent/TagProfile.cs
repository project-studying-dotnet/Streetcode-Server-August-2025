using AutoMapper;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.DAL.Entities.AdditionalContent;

namespace Streetcode.BLL.Mapping.AdditionalContent;

public class TagProfile : Profile
{
    public TagProfile()
    {
        CreateMap<Tag, TagDTO>().ForMember(x => x.Streetcodes, conf => conf.Ignore());
        CreateMap<Tag, StreetcodeTagDTO>().ReverseMap();
        CreateMap<StreetcodeTagIndex, StreetcodeTagDTO>()
            .ForMember(x => x.Id, conf => conf.MapFrom(ti => ti.TagId))
            .ForMember(x => x.Title, conf => conf.MapFrom(ti => ti.Tag!.Title ?? ""));

        CreateMap<StreetcodeTagUpdateDTO, StreetcodeTagIndex>()
            .ForMember(dest => dest.TagId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src.Id <= 0 ? new Tag() { Id = src.Id, Title = src.Title } : null));

        CreateMap<StreetcodeTagUpdateDTO, Tag>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));

        CreateMap<StreetcodeTagDTO, StreetcodeTagUpdateDTO>().ReverseMap();
        CreateMap<Tag, StreetcodeTagUpdateDTO>().ReverseMap();
    }
}
