using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;

namespace Streetcode.BLL.MediatR.Comments.GetById
{
    public record GetCommentByIdQuery(int CommentId, bool? IsReviewed = null)
    : IRequest<Result<CommentDTO>>;
}
