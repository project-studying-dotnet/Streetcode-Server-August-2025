using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent;

namespace Streetcode.BLL.MediatR.Streetcode.Term.Update
{
    public record UpdateTermCommand(int Id, CreateTermDTO TermDTO) : IRequest<Result<TermDTO>>;
}
