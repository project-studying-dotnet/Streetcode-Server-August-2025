using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;

namespace Streetcode.BLL.MediatR.Comments.GetAll
{
    public record GetAllCommentsForAdminQuery(bool? IsReviewed = null)
    : IRequest<Result<IEnumerable<CommentDTO>>>;
}
