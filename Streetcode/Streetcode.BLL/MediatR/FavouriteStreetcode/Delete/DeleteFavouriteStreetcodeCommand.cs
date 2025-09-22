using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.FavouriteStreetcode;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.MediatR.FavouriteStreetcode.Delete;

public record DeleteFavouriteStreetcodeCommand(int Id, int RequestingUserId, UserRole UserRole)
    : IRequest<Result<FavouriteStreetcodeDTO>>;