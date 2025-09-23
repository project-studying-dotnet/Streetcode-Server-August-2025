using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.MediatR.FavouriteStreetcode.GetFavoritesByUserId
{
    public record GetFavoritesByUserIdQuery(int UserId) : IRequest<Result<IEnumerable<StreetcodeDTO>>>;
}
