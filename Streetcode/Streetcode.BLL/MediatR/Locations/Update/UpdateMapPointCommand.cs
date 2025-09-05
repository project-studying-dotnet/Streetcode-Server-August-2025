using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Locations.Update;

public record UpdateMapPointCommand(int Id)
    : IRequest<Result<Unit>>;
