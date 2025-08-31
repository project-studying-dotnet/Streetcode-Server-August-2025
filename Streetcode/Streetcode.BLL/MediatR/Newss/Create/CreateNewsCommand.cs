using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;

namespace Streetcode.BLL.MediatR.Newss.Create
{
    public record CreateNewsCommand(NewsDTO NewNews) : IRequest<Result<NewsDTO>>;
}
