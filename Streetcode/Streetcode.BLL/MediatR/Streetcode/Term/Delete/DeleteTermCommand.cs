using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Term.Delete
{
    public record DeleteTermCommand(int Id) : IRequest<Result<Unit>>;
}
