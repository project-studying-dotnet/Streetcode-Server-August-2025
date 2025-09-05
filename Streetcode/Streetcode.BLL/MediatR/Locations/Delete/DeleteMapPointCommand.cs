using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Locations.Delete;

public record DeleteMapPointCommand(int Id)
    : IRequest<Result<Unit>>;
