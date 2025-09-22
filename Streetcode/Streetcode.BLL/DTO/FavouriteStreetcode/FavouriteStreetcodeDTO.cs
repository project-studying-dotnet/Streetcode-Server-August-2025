using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.BLL.DTO.FavouriteStreetcode;

public class FavouriteStreetcodeDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int StreetcodeId { get; set; }

    public User? User { get; set; }
    public StreetcodeContent? Streetcode { get; set; }
}
