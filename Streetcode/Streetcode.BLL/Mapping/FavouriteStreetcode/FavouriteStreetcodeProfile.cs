using AutoMapper;
using Streetcode.BLL.DTO.FavouriteStreetcode;

namespace Streetcode.BLL.Mapping.FavouriteStreetcode;

public class FavouriteStreetcodeProfile: Profile
{
    public FavouriteStreetcodeProfile()
    {
        CreateMap<DAL.Entities.Favourite.FavouriteStreetcode, FavouriteStreetcodeDTO>().ReverseMap();
    }
}