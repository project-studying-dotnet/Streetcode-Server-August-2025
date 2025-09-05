using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Locations;

namespace Streetcode.BLL.MediatR.Locations.GetAll;

public record GetAllMapPointsQuery : IRequest<Result<IEnumerable<MapPointDTO>>>;